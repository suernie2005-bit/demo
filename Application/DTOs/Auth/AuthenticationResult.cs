namespace QLSanPham.Application.DTOs.Auth;

public sealed record AuthenticationResult(
    bool Succeeded,
    AuthenticationTokenResponse? Tokens)
{
    public static AuthenticationResult InvalidCredentials { get; } = new(false, null);

    public static AuthenticationResult Success(AuthenticationTokenResponse tokens)
    {
        return new AuthenticationResult(true, tokens);
    }
}
