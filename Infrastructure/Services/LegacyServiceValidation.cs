using FluentValidation;

namespace QLSanPham.Infrastructure.Services;

internal static class LegacyServiceValidation
{
    public static void EnsureValid<TRequest>(
        TRequest request,
        IValidator<TRequest> validator)
    {
        var result = validator.Validate(request);

        if (!result.IsValid)
        {
            throw new ArgumentException(result.Errors[0].ErrorMessage);
        }
    }
}
