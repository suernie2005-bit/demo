namespace QLSanPham.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;

    public byte[] GetSigningKeyBytes()
    {
        Validate();
        return Convert.FromBase64String(SigningKey);
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new InvalidOperationException("Thiếu cấu hình Jwt:Issuer.");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("Thiếu cấu hình Jwt:Audience.");
        }

        if (string.IsNullOrWhiteSpace(SigningKey))
        {
            throw new InvalidOperationException(
                "Thiếu Jwt:SigningKey. Hãy cấu hình key cục bộ bằng User Secrets.");
        }

        byte[] key;
        try
        {
            key = Convert.FromBase64String(SigningKey);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey phải là chuỗi Base64 hợp lệ chứa tối thiểu 32 byte.",
                exception);
        }

        if (key.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey phải chứa tối thiểu 32 byte ngẫu nhiên.");
        }

        if (AccessTokenMinutes is < 1 or > 60)
        {
            throw new InvalidOperationException("Jwt:AccessTokenMinutes phải nằm trong khoảng 1–60.");
        }

        if (RefreshTokenDays is < 1 or > 30)
        {
            throw new InvalidOperationException("Jwt:RefreshTokenDays phải nằm trong khoảng 1–30.");
        }
    }
}
