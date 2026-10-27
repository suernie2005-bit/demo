using QLSanPham.Application.Abstractions.Identity;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.Auth;
using QLSanPham.Application.Features.Auth.Commands;
using QLSanPham.Domain.Entities;

namespace WebAPI.Tests;

public sealed class AuthenticationRefreshFlowTests
{
    [Fact]
    public async Task Refresh_rotates_token_and_links_the_replacement()
    {
        DateTimeOffset now = new(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);
        Guid familyId = Guid.NewGuid();
        var oldToken = new RefreshToken
        {
            UserId = "user-1",
            TokenHash = "hash:old-token",
            FamilyId = familyId,
            CreatedAtUtc = now.AddDays(-1).UtcDateTime,
            ExpiresAtUtc = now.AddDays(6).UtcDateTime
        };
        var repository = new InMemoryRefreshTokenRepository(oldToken);
        var tokenService = new StubAuthenticationTokenService(
            new AuthenticationTokenIssue(
                new AuthenticationTokenResponse(
                    "access-token",
                    "Bearer",
                    now.AddMinutes(15).UtcDateTime,
                    "new-refresh-token",
                    now.AddDays(7).UtcDateTime),
                "hash:new-refresh-token",
                now.UtcDateTime));
        var handler = new RefreshTokenCommandHandler(
            new StubUserAccountService(),
            tokenService,
            repository,
            new FixedTimeProvider(now));

        AuthenticationResult result = await handler.Handle(
            new RefreshTokenCommand("old-token"),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("new-refresh-token", result.Tokens!.RefreshToken);
        Assert.Equal(now.UtcDateTime, oldToken.RevokedAtUtc);
        Assert.NotNull(oldToken.ReplacedByTokenId);
        Assert.Same(oldToken.ReplacedByToken, repository.Tokens.Single(token => token.TokenHash == "hash:new-refresh-token"));
        Assert.DoesNotContain(repository.Tokens, token => token.TokenHash == "new-refresh-token");
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task Reuse_of_revoked_token_revokes_all_active_tokens_in_family()
    {
        DateTimeOffset now = new(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);
        Guid familyId = Guid.NewGuid();
        var reusedToken = new RefreshToken
        {
            UserId = "user-1",
            TokenHash = "hash:reused-token",
            FamilyId = familyId,
            CreatedAtUtc = now.AddDays(-1).UtcDateTime,
            ExpiresAtUtc = now.AddDays(6).UtcDateTime,
            RevokedAtUtc = now.AddHours(-1).UtcDateTime
        };
        var activeToken = new RefreshToken
        {
            UserId = "user-1",
            TokenHash = "hash:active-token",
            FamilyId = familyId,
            CreatedAtUtc = now.AddMinutes(-1).UtcDateTime,
            ExpiresAtUtc = now.AddDays(7).UtcDateTime
        };
        var repository = new InMemoryRefreshTokenRepository(reusedToken, activeToken);
        var handler = new RefreshTokenCommandHandler(
            new StubUserAccountService(),
            new StubAuthenticationTokenService(),
            repository,
            new FixedTimeProvider(now));

        AuthenticationResult result = await handler.Handle(
            new RefreshTokenCommand("reused-token"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Null(result.Tokens);
        Assert.Equal(now.UtcDateTime, activeToken.RevokedAtUtc);
        Assert.Equal(1, repository.SaveCount);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class InMemoryRefreshTokenRepository(params RefreshToken[] tokens)
        : IRefreshTokenRepository
    {
        public List<RefreshToken> Tokens { get; } = tokens.ToList();
        public int SaveCount { get; private set; }

        public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken)
        {
            return Task.FromResult(Tokens.SingleOrDefault(token => token.TokenHash == tokenHash));
        }

        public Task<IReadOnlyList<RefreshToken>> GetByFamilyIdAsync(
            Guid familyId,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<RefreshToken> family = Tokens.Where(token => token.FamilyId == familyId).ToArray();
            return Task.FromResult(family);
        }

        public void Add(RefreshToken refreshToken) => Tokens.Add(refreshToken);

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class StubAuthenticationTokenService(AuthenticationTokenIssue? issue = null)
        : IAuthenticationTokenService
    {
        public AuthenticationTokenIssue Issue(
            UserAccount user,
            IReadOnlyCollection<string> roles,
            Guid familyId)
        {
            return issue ?? throw new InvalidOperationException("Token issue was not expected.");
        }

        public string HashRefreshToken(string rawToken) => $"hash:{rawToken}";
    }

    private sealed class StubUserAccountService : IUserAccountService
    {
        public Task<UserAccount?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<UserAccount?>(new UserAccount(userId, "user@example.test", "user@example.test"));
        }

        public Task<IReadOnlyList<string>> GetRolesAsync(string userId, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<string>>(new[] { RoleNames.User });
        }

        public Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<UserAccountCreationResult> CreateAsync(string email, string password, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<bool> CheckPasswordAsync(string userId, string password, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<bool> IsInRoleAsync(string userId, string role, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<UserAccountOperationResult> EnsureRoleExistsAsync(string role, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<UserAccountOperationResult> AddToRoleAsync(string userId, string role, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
