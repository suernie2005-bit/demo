namespace QLSanPham.Application.Abstractions.Identity;

/// <summary>
/// Application-facing user and role operations without an ASP.NET Identity dependency.
/// </summary>
public interface IUserAccountService
{
    Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<UserAccount?> FindByIdAsync(string userId, CancellationToken cancellationToken);

    Task<UserAccountCreationResult> CreateAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<bool> CheckPasswordAsync(
        string userId,
        string password,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetRolesAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<bool> IsInRoleAsync(
        string userId,
        string role,
        CancellationToken cancellationToken);

    Task<UserAccountOperationResult> EnsureRoleExistsAsync(
        string role,
        CancellationToken cancellationToken);

    Task<UserAccountOperationResult> AddToRoleAsync(
        string userId,
        string role,
        CancellationToken cancellationToken);
}
