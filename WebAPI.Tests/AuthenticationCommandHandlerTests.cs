using MediatR;
using QLSanPham.Application.Abstractions.Identity;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.Auth;
using QLSanPham.Application.Features.Auth.Commands;
using QLSanPham.Domain.Entities;

namespace WebAPI.Tests;

public sealed class AuthenticationCommandHandlerTests
{
    [Fact]
    public async Task Register_creates_role_definitions_and_assigns_only_user_role()
    {
        var users = new FakeUserAccountService();
        var handler = new RegisterAccountCommandHandler(users);

        RegisteredAccountDto result = await handler.Handle(
            new RegisterAccountCommand(" user@example.test ", "StrongPass1!"),
            CancellationToken.None);

        Assert.Equal("user-1", result.Id);
        Assert.Equal(new[] { RoleNames.User, RoleNames.Admin }, users.EnsuredRoles);
        Assert.Equal(new[] { RoleNames.User }, users.AssignedRoles);
    }

    [Fact]
    public async Task Register_returns_conflict_for_duplicate_email()
    {
        var users = new FakeUserAccountService
        {
            CreationResult = new UserAccountCreationResult(false, true, new[] { "DuplicateEmail" })
        };
        var handler = new RegisterAccountCommandHandler(users);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new RegisterAccountCommand("user@example.test", "StrongPass1!"),
            CancellationToken.None));
        Assert.Empty(users.AssignedRoles);
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_generic_failure_without_issuing_tokens()
    {
        var users = new FakeUserAccountService { PasswordIsValid = false };
        var tokenService = new FakeAuthenticationTokenService();
        var repository = new FakeRefreshTokenRepository();
        var handler = new LoginCommandHandler(users, tokenService, repository);

        AuthenticationResult result = await handler.Handle(
            new LoginCommand("user@example.test", "wrong"),
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Null(result.Tokens);
        Assert.Equal(0, tokenService.IssueCount);
        Assert.Empty(repository.Tokens);
    }

    [Fact]
    public async Task Login_success_stores_hash_and_creates_a_refresh_family()
    {
        var users = new FakeUserAccountService { PasswordIsValid = true };
        var tokenService = new FakeAuthenticationTokenService(CreateIssue("refresh-1", "hash-1"));
        var repository = new FakeRefreshTokenRepository();
        var handler = new LoginCommandHandler(users, tokenService, repository);

        AuthenticationResult result = await handler.Handle(
            new LoginCommand("user@example.test", "StrongPass1!"),
            CancellationToken.None);

        var saved = Assert.Single(repository.Tokens);
        Assert.True(result.Succeeded);
        Assert.Equal("refresh-1", result.Tokens!.RefreshToken);
        Assert.Equal("hash-1", saved.TokenHash);
        Assert.NotEqual(Guid.Empty, saved.FamilyId);
        Assert.Equal(1, repository.SaveCount);
    }

    [Fact]
    public async Task Logout_revokes_all_active_tokens_in_the_family()
    {
        DateTime now = new(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc);
        Guid familyId = Guid.NewGuid();
        var active = new RefreshToken
        {
            UserId = "user-1",
            TokenHash = "hash:refresh-1",
            FamilyId = familyId,
            CreatedAtUtc = now.AddDays(-1),
            ExpiresAtUtc = now.AddDays(6)
        };
        var secondActive = new RefreshToken
        {
            UserId = "user-1",
            TokenHash = "hash:refresh-2",
            FamilyId = familyId,
            CreatedAtUtc = now.AddHours(-1),
            ExpiresAtUtc = now.AddDays(7)
        };
        var repository = new FakeRefreshTokenRepository(active, secondActive);
        var handler = new LogoutCommandHandler(
            new FakeAuthenticationTokenService(),
            repository,
            new FixedTimeProvider(now));

        await handler.Handle(new LogoutCommand("refresh-1"), CancellationToken.None);

        Assert.Equal(now, active.RevokedAtUtc);
        Assert.Equal(now, secondActive.RevokedAtUtc);
        Assert.Equal(1, repository.SaveCount);
    }

    private static AuthenticationTokenIssue CreateIssue(string refreshToken, string hash)
    {
        DateTime now = new(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc);
        return new AuthenticationTokenIssue(
            new AuthenticationTokenResponse("access", "Bearer", now.AddMinutes(15), refreshToken, now.AddDays(7)),
            hash,
            now);
    }

    private sealed class FakeUserAccountService : IUserAccountService
    {
        public UserAccount Account { get; } = new("user-1", "user@example.test", "user@example.test");
        public UserAccountCreationResult CreationResult { get; set; } = new(true, false, Array.Empty<string>());
        public bool PasswordIsValid { get; set; } = true;
        public List<string> EnsuredRoles { get; } = new();
        public List<string> AssignedRoles { get; } = new();

        public Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
            Task.FromResult<UserAccount?>(Account);

        public Task<UserAccount?> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
            Task.FromResult<UserAccount?>(Account);

        public Task<UserAccountCreationResult> CreateAsync(string email, string password, CancellationToken cancellationToken) =>
            Task.FromResult(CreationResult);

        public Task<bool> CheckPasswordAsync(string userId, string password, CancellationToken cancellationToken) =>
            Task.FromResult(PasswordIsValid);

        public Task<IReadOnlyList<string>> GetRolesAsync(string userId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<string>>(new[] { RoleNames.User });

        public Task<bool> IsInRoleAsync(string userId, string role, CancellationToken cancellationToken) =>
            Task.FromResult(AssignedRoles.Contains(role, StringComparer.Ordinal));

        public Task<UserAccountOperationResult> EnsureRoleExistsAsync(string role, CancellationToken cancellationToken)
        {
            EnsuredRoles.Add(role);
            return Task.FromResult(new UserAccountOperationResult(true, Array.Empty<string>()));
        }

        public Task<UserAccountOperationResult> AddToRoleAsync(string userId, string role, CancellationToken cancellationToken)
        {
            AssignedRoles.Add(role);
            return Task.FromResult(new UserAccountOperationResult(true, Array.Empty<string>()));
        }
    }

    private sealed class FakeAuthenticationTokenService(AuthenticationTokenIssue? issue = null)
        : IAuthenticationTokenService
    {
        public int IssueCount { get; private set; }

        public AuthenticationTokenIssue Issue(UserAccount user, IReadOnlyCollection<string> roles, Guid familyId)
        {
            IssueCount++;
            return issue ?? CreateIssue("refresh-default", "hash-default");
        }

        public string HashRefreshToken(string rawToken) => $"hash:{rawToken}";
    }

    private sealed class FakeRefreshTokenRepository(params RefreshToken[] tokens)
        : IRefreshTokenRepository
    {
        public List<RefreshToken> Tokens { get; } = tokens.ToList();
        public int SaveCount { get; private set; }

        public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
            Task.FromResult(Tokens.SingleOrDefault(token => token.TokenHash == tokenHash));

        public Task<IReadOnlyList<RefreshToken>> GetByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken)
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

    private sealed class FixedTimeProvider(DateTime now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(now);
    }
}
