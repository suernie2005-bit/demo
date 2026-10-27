using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QLSanPham.Application.Abstractions.Identity;
using QLSanPham.Infrastructure.Persistence;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.Abstractions.Transactions;
using QLSanPham.Infrastructure.Identity;
using QLSanPham.Infrastructure.Authentication;
using QLSanPham.Infrastructure.Persistence.Repositories;

namespace QLSanPham.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Chưa cấu hình ConnectionStrings:DefaultConnection.");
        }

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        jwtOptions.Validate();
        services.AddSingleton(jwtOptions);
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<IAuthenticationTokenService, JwtAuthenticationTokenService>();

        services.AddDbContext<QLSanPhamDBContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddIdentityCore<ApplicationUser>(options =>
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

        services.AddScoped<ILoaiRepository, LoaiRepository>();
        services.AddScoped<ISanPhamRepository, SanPhamRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ITransactionManager, EfCoreTransactionManager>();
        services.AddScoped<IUserAccountService, IdentityUserAccountService>();

        return services;
    }
}
