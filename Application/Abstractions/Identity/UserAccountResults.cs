namespace QLSanPham.Application.Abstractions.Identity;

public sealed record UserAccountCreationResult(
    bool Succeeded,
    bool Conflict,
    IReadOnlyList<string> ErrorCodes);

public sealed record UserAccountOperationResult(
    bool Succeeded,
    IReadOnlyList<string> ErrorCodes);
