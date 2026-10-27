using QLSanPham.Application.Features.Auth.Commands;
using QLSanPham.Application.Features.Auth.Validation;

namespace WebAPI.Tests;

public sealed class AuthenticationRequestValidatorTests
{
    [Fact]
    public void Register_accepts_valid_email_and_strong_password()
    {
        var validator = new RegisterAccountCommandValidator();

        var result = validator.Validate(new RegisterAccountCommand("user@example.test", "StrongPass1!"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Register_rejects_invalid_email_and_weak_password()
    {
        var validator = new RegisterAccountCommandValidator();

        var result = validator.Validate(new RegisterAccountCommand("not-an-email", "weak"));

        Assert.Contains(result.Errors, error => error.PropertyName == "Email");
        Assert.Contains(result.Errors, error => error.PropertyName == "Password");
    }

    [Theory]
    [InlineData("")]
    [InlineData("short")]
    public void Refresh_rejects_empty_or_too_short_token(string token)
    {
        var validator = new RefreshTokenCommandValidator();

        var result = validator.Validate(new RefreshTokenCommand(token));

        Assert.Contains(result.Errors, error => error.PropertyName == "RefreshToken");
    }

    [Fact]
    public void Login_rejects_missing_password()
    {
        var validator = new LoginCommandValidator();

        var result = validator.Validate(new LoginCommand("user@example.test", ""));

        Assert.Contains(result.Errors, error => error.PropertyName == "Password");
    }
}
