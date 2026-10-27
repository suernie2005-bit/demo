using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using QLSanPham.Application;
using QLSanPham.Application.Abstractions.Identity;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.Abstractions.Transactions;
using QLSanPham.Application.DTOs.Loai;
using QLSanPham.Application.DTOs.Auth;
using QLSanPham.Infrastructure.Identity;
using QLSanPham.Infrastructure.Authentication;
using QLSanPham.Infrastructure.Persistence;
using QLSanPham.Infrastructure.Persistence.Repositories;
using QLSanPham.Domain.Entities;
using WebAPI.Authorization;
using WebAPI.Controllers;

namespace WebAPI.Tests;

public sealed class WebApiAuthorizationIntegrationTests
{
    [Fact]
    public async Task Public_catalog_get_returns_seeded_data_and_maps_related_name()
    {
        await using var host = await TestApiHost.CreateAsync();

        using var response = await host.Client.GetAsync("/api/sanpham/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = await System.Text.Json.JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        Assert.Equal("Điện tử", body.RootElement.GetProperty("tenLoai").GetString());
    }

    [Fact]
    public async Task Swagger_documents_bearer_only_for_protected_catalog_operations()
    {
        await using var host = await TestApiHost.CreateAsync();

        using var response = await host.Client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
        using var document = System.Text.Json.JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var root = document.RootElement;
        Assert.True(root.TryGetProperty("components", out var components), root.ToString());
        Assert.True(components.TryGetProperty("securitySchemes", out var securitySchemes), root.ToString());
        Assert.True(securitySchemes.TryGetProperty("Bearer", out var bearerScheme), root.ToString());
        Assert.Equal(
            "bearer",
            bearerScheme
                .GetProperty("scheme")
                .GetString());

        var paths = root.GetProperty("paths");
        var loaiPath = paths.EnumerateObject()
            .Single(path => path.Name.Equals("/api/Loai", StringComparison.OrdinalIgnoreCase))
            .Value;
        Assert.True(loaiPath.GetProperty("post").TryGetProperty("security", out _));
        Assert.False(loaiPath.GetProperty("get").TryGetProperty("security", out _));
        Assert.False(paths.GetProperty("/api/auth/login").GetProperty("post").TryGetProperty("security", out _));
    }

    [Fact]
    public async Task Catalog_write_without_token_returns_401()
    {
        await using var host = await TestApiHost.CreateAsync();

        using var response = await host.Client.PostAsJsonAsync(
            "/api/loai",
            new CreateLoaiDto { Ten = "Không được tạo" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Catalog_write_with_malformed_token_returns_401()
    {
        await using var host = await TestApiHost.CreateAsync();
        host.SetBearerToken("not-a-jwt");

        using var response = await host.Client.PostAsJsonAsync(
            "/api/loai",
            new CreateLoaiDto { Ten = "Không được tạo" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Catalog_write_with_authenticated_user_role_returns_403()
    {
        await using var host = await TestApiHost.CreateAsync();
        host.SetBearerToken(host.CreateAccessToken(RoleNames.User));

        using var response = await host.Client.PostAsJsonAsync(
            "/api/loai",
            new CreateLoaiDto { Ten = "Không được tạo" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Catalog_write_with_admin_token_returns_201()
    {
        await using var host = await TestApiHost.CreateAsync();
        host.SetBearerToken(host.CreateAccessToken(RoleNames.Admin));

        using var response = await host.Client.PostAsJsonAsync(
            "/api/loai",
            new CreateLoaiDto { Ten = "Loại tạo trong InMemory test" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Catalog_write_with_expired_token_returns_401()
    {
        await using var host = await TestApiHost.CreateAsync();
        host.SetBearerToken(host.CreateAccessToken(
            RoleNames.Admin,
            DateTimeOffset.UtcNow.AddMinutes(-30)));

        using var response = await host.Client.PostAsJsonAsync(
            "/api/loai",
            new CreateLoaiDto { Ten = "Không được tạo" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_cannot_assign_admin_role_from_public_request()
    {
        await using var host = await TestApiHost.CreateAsync();
        using var register = await host.Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "role-test@example.test",
            password = "StrongPass1!",
            role = RoleNames.Admin
        });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        using var login = await host.Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "role-test@example.test",
            password = "StrongPass1!"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        AuthenticationTokenResponse tokens = (await login.Content.ReadFromJsonAsync<AuthenticationTokenResponse>())!;
        host.SetBearerToken(tokens.AccessToken);

        using var write = await host.Client.PostAsJsonAsync(
            "/api/loai",
            new CreateLoaiDto { Ten = "Không được cấp Admin" });

        Assert.Equal(HttpStatusCode.Forbidden, write.StatusCode);
    }

    [Fact]
    public async Task Login_returns_401_for_wrong_password_and_tokens_for_valid_password()
    {
        await using var host = await TestApiHost.CreateAsync();
        await RegisterAsync(host.Client, "login-test@example.test");
        var passwordCheck = await host.CheckStoredPasswordAsync(
            "login-test@example.test",
            "StrongPass1!");
        Assert.True(passwordCheck.Found);
        Assert.True(passwordCheck.PasswordMatches);

        using var invalidLogin = await host.Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "login-test@example.test",
            password = "WrongPass1!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, invalidLogin.StatusCode);

        using var validLogin = await host.Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "login-test@example.test",
            password = "StrongPass1!"
        });
        Assert.Equal(HttpStatusCode.OK, validLogin.StatusCode);
        AuthenticationTokenResponse tokens = (await validLogin.Content.ReadFromJsonAsync<AuthenticationTokenResponse>())!;
        Assert.Equal("Bearer", tokens.TokenType);
        Assert.False(string.IsNullOrWhiteSpace(tokens.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));
    }

    [Fact]
    public async Task Refresh_rotates_token_and_replay_revokes_the_new_family_token()
    {
        await using var host = await TestApiHost.CreateAsync();
        await RegisterAsync(host.Client, "refresh-test@example.test");
        AuthenticationTokenResponse initial = await LoginAsync(host.Client, "refresh-test@example.test");

        using var rotatedResponse = await host.Client.PostAsJsonAsync(
            "/api/auth/refresh",
            new { refreshToken = initial.RefreshToken });
        Assert.Equal(HttpStatusCode.OK, rotatedResponse.StatusCode);
        AuthenticationTokenResponse rotated = (await rotatedResponse.Content.ReadFromJsonAsync<AuthenticationTokenResponse>())!;
        Assert.NotEqual(initial.RefreshToken, rotated.RefreshToken);

        using var replayResponse = await host.Client.PostAsJsonAsync(
            "/api/auth/refresh",
            new { refreshToken = initial.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, replayResponse.StatusCode);

        using var familyRevokedResponse = await host.Client.PostAsJsonAsync(
            "/api/auth/refresh",
            new { refreshToken = rotated.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, familyRevokedResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_returns_204_and_revokes_refresh_token()
    {
        await using var host = await TestApiHost.CreateAsync();
        await RegisterAsync(host.Client, "logout-test@example.test");
        AuthenticationTokenResponse tokens = await LoginAsync(host.Client, "logout-test@example.test");

        using var logout = await host.Client.PostAsJsonAsync(
            "/api/auth/logout",
            new { refreshToken = tokens.RefreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        using var refresh = await host.Client.PostAsJsonAsync(
            "/api/auth/refresh",
            new { refreshToken = tokens.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    private static async Task RegisterAsync(HttpClient client, string email)
    {
        using var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "StrongPass1!"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private static async Task<AuthenticationTokenResponse> LoginAsync(HttpClient client, string email)
    {
        using var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "StrongPass1!"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<AuthenticationTokenResponse>())!;
    }

    private sealed class TestApiHost : IAsyncDisposable
    {
        private readonly WebApplication _application;
        private readonly JwtOptions _jwtOptions;

        private TestApiHost(WebApplication application, HttpClient client, JwtOptions jwtOptions)
        {
            _application = application;
            Client = client;
            _jwtOptions = jwtOptions;
        }

        public HttpClient Client { get; }

        public static async Task<TestApiHost> CreateAsync()
        {
            JwtOptions jwtOptions = JwtAuthenticationTokenServiceTests.CreateOptions();
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseTestServer();

            builder.Services.AddSingleton(jwtOptions);
            builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
            builder.Services.AddControllers()
                .AddApplicationPart(typeof(LoaiController).Assembly);
            builder.Services.AddApiDocumentation();
            builder.Services.AddApplication();
            string databaseName = $"m7-api-{Guid.NewGuid():N}";
            var databaseRoot = new InMemoryDatabaseRoot();
            builder.Services.AddDbContext<QLSanPhamDBContext>(options =>
                options.UseInMemoryDatabase(databaseName, databaseRoot));
            builder.Services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                })
                .AddRoles<IdentityRole>()
                .AddSignInManager()
                .AddEntityFrameworkStores<QLSanPhamDBContext>();
            builder.Services.AddScoped<IUserAccountService, IdentityUserAccountService>();
            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            builder.Services.AddSingleton<IAuthenticationTokenService, JwtAuthenticationTokenService>();
            builder.Services.AddScoped<ILoaiRepository, LoaiRepository>();
            builder.Services.AddScoped<ISanPhamRepository, SanPhamRepository>();
            builder.Services.AddScoped<ITransactionManager, InMemoryTransactionManager>();
            builder.Services.AddJwtAuthenticationAndCatalogAuthorization();

            var app = builder.Build();
            app.UseSwagger();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            await using (var scope = app.Services.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<QLSanPhamDBContext>();
                await dbContext.Database.EnsureCreatedAsync();
                if (!await dbContext.Loais.AnyAsync())
                {
                    dbContext.Loais.Add(new Loai { Id = 1, Ten = "Điện tử" });
                }

                if (!await dbContext.SanPhams.AnyAsync())
                {
                    dbContext.SanPhams.Add(new SanPham
                    {
                        Id = 1,
                        TenSp = "Tai nghe Bluetooth",
                        Gia = 590000m,
                        SoLuong = 20,
                        LoaiId = 1,
                        Ngay = DateTime.UtcNow
                    });
                }

                await dbContext.SaveChangesAsync();
            }

            await app.StartAsync();
            return new TestApiHost(app, app.GetTestClient(), jwtOptions);
        }

        public void SetBearerToken(string token)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<(bool Found, bool PasswordMatches)> CheckStoredPasswordAsync(
            string email,
            string password)
        {
            await using var scope = _application.Services.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            return (user is not null,
                user is not null && await userManager.CheckPasswordAsync(user, password));
        }

        public string CreateAccessToken(string role, DateTimeOffset? issuedAtUtc = null)
        {
            var tokenService = new JwtAuthenticationTokenService(
                _jwtOptions,
                new FixedTimeProvider(issuedAtUtc ?? DateTimeOffset.UtcNow));

            return tokenService.Issue(
                new UserAccount("test-user", "user@example.test", "user@example.test"),
                new[] { role },
                Guid.NewGuid()).Response.AccessToken;
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _application.DisposeAsync();
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class InMemoryTransactionManager : ITransactionManager
    {
        public Task<TResponse> ExecuteAsync<TResponse>(
            Func<CancellationToken, Task<TResponse>> operation,
            CancellationToken cancellationToken)
        {
            return operation(cancellationToken);
        }
    }
}
