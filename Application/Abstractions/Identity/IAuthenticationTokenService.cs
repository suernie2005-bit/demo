using QLSanPham.Application.DTOs.Auth;

namespace QLSanPham.Application.Abstractions.Identity;

public sealed record AuthenticationTokenIssue(
    AuthenticationTokenResponse Response,
    string RefreshTokenHash,
    DateTime RefreshTokenCreatedAtUtc);

public interface IAuthenticationTokenService
{
    AuthenticationTokenIssue Issue(
        UserAccount user,
        IReadOnlyCollection<string> roles,
        Guid familyId);

    string HashRefreshToken(string rawToken);
}
