using System.IdentityModel.Tokens.Jwt;
using QLSanPham.Application.Abstractions.Identity;
using QLSanPham.Infrastructure.Authentication;

namespace WebAPI.Tests;

public sealed class JwtAuthenticationTokenServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Issue_creates_access_token_with_expected_claims_and_expiration()
    {
        var options = CreateOptions();
        var service = new JwtAuthenticationTokenService(options, new FixedTimeProvider(FixedNow));

        var issue = service.Issue(
            new UserAccount("user-123", "user@example.test", "user@example.test"),
            new[] { RoleNames.User, RoleNames.Admin },
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(issue.Response.AccessToken);

        Assert.Equal(options.Issuer, jwt.Issuer);
        Assert.Contains(options.Audience, jwt.Audiences);
        Assert.Equal("user-123", jwt.Subject);
        Assert.Contains(jwt.Claims, claim => claim.Type == "role" && claim.Value == RoleNames.Admin);
        Assert.Contains(jwt.Claims, claim => claim.Type == "sid" && claim.Value == "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        Assert.Equal(FixedNow.AddMinutes(15).UtcDateTime, issue.Response.AccessTokenExpiresAtUtc);
        Assert.Equal(FixedNow.UtcDateTime, issue.RefreshTokenCreatedAtUtc);
    }

    [Fact]
    public void Issue_creates_random_refresh_token_and_only_exposes_its_hash_separately()
    {
        var service = new JwtAuthenticationTokenService(CreateOptions(), new FixedTimeProvider(FixedNow));
        var account = new UserAccount("user-123", "user@example.test", "user@example.test");

        var first = service.Issue(account, new[] { RoleNames.User }, Guid.NewGuid());
        var second = service.Issue(account, new[] { RoleNames.User }, Guid.NewGuid());

        Assert.NotEqual(first.Response.RefreshToken, second.Response.RefreshToken);
        Assert.Equal(64, first.RefreshTokenHash.Length);
        Assert.Equal(first.RefreshTokenHash, service.HashRefreshToken(first.Response.RefreshToken));
        Assert.DoesNotContain(first.Response.RefreshToken, first.RefreshTokenHash);
        Assert.Equal(FixedNow.AddDays(7).UtcDateTime, first.Response.RefreshTokenExpiresAtUtc);
    }

    [Fact]
    public void Jwt_options_reject_a_signing_key_shorter_than_256_bits()
    {
        var options = CreateOptions();
        options.SigningKey = Convert.ToBase64String(new byte[31]);

        Assert.Throws<InvalidOperationException>(() => options.Validate());
    }

    [Fact]
    public void Jwt_options_accept_a_512_bit_random_signing_key()
    {
        var options = CreateOptions();

        options.Validate();

        Assert.Equal(64, options.GetSigningKeyBytes().Length);
    }

    internal static JwtOptions CreateOptions()
    {
        return new JwtOptions
        {
            Issuer = "QLSanPham.Tests",
            Audience = "QLSanPham.TestClient",
            SigningKey = Convert.ToBase64String(Enumerable.Range(1, 64).Select(value => (byte)value).ToArray()),
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        };
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
