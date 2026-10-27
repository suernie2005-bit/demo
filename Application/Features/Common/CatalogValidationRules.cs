using FluentValidation;

namespace QLSanPham.Application.Features.Common;

internal static class CatalogValidationRules
{
    public static void RequiredTrimmedName<T>(
        IRuleBuilderInitial<T, string> rule,
        string displayName,
        int maximumLength)
    {
        rule.Cascade(CascadeMode.Stop)
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage($"{displayName} không được để trống.")
            .Must(value => value.Trim().Length <= maximumLength)
            .WithMessage($"{displayName} không được vượt quá {maximumLength} ký tự.");
    }

    public static bool IsAllowedValue(string? value, params string[] allowedValues)
    {
        return value is not null
            && allowedValues.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);
    }
}
