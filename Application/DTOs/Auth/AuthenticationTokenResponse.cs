namespace QLSanPham.Application.DTOs.Auth;

public sealed record AuthenticationTokenResponse(
    string AccessToken,
    string TokenType,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);
