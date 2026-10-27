using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using QLSanPham.Application.Abstractions.Identity;
using QLSanPham.Application.DTOs.Auth;

namespace QLSanPham.Infrastructure.Authentication;

public sealed class JwtAuthenticationTokenService : IAuthenticationTokenService
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly SigningCredentials _signingCredentials;

    public JwtAuthenticationTokenService(JwtOptions options, TimeProvider timeProvider)
    {
        _options = options;
        _options.Validate();
        _timeProvider = timeProvider;
        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(_options.GetSigningKeyBytes()),
            SecurityAlgorithms.HmacSha256);
    }

    public AuthenticationTokenIssue Issue(
        UserAccount user,
        IReadOnlyCollection<string> roles,
        Guid familyId)
    {
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        DateTime accessExpiresAt = now.AddMinutes(_options.AccessTokenMinutes);
        DateTime refreshExpiresAt = now.AddDays(_options.RefreshTokenDays);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("sid", familyId.ToString("N"))
        };

        claims.AddRange(roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(role => new Claim("role", role)));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: accessExpiresAt,
            signingCredentials: _signingCredentials);

        string accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        string refreshToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        string refreshTokenHash = HashRefreshToken(refreshToken);

        return new AuthenticationTokenIssue(
            new AuthenticationTokenResponse(
                accessToken,
                "Bearer",
                accessExpiresAt,
                refreshToken,
                refreshExpiresAt),
            refreshTokenHash,
            now);
    }

    public string HashRefreshToken(string rawToken)
    {
        byte[] hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
