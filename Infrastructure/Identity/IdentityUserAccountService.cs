using Microsoft.AspNetCore.Identity;
using QLSanPham.Application.Abstractions.Identity;

namespace QLSanPham.Infrastructure.Identity;

public sealed class IdentityUserAccountService : IUserAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityUserAccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    public async Task<UserAccount?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByEmailAsync(email);
        cancellationToken.ThrowIfCancellationRequested();

        return user is null ? null : ToAccount(user);
    }

    public async Task<UserAccount?> FindByIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByIdAsync(userId);
        cancellationToken.ThrowIfCancellationRequested();

        return user is null ? null : ToAccount(user);
    }

    public async Task<UserAccountCreationResult> CreateAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string normalizedEmail = email.Trim();
        var user = new ApplicationUser
        {
            Email = normalizedEmail,
            UserName = normalizedEmail,
            EmailConfirmed = false
        };

        var identityResult = await _userManager.CreateAsync(user, password);
        cancellationToken.ThrowIfCancellationRequested();

        string[] errorCodes = identityResult.Errors
            .Select(error => error.Code)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        bool conflict = errorCodes.Any(code =>
            string.Equals(code, "DuplicateEmail", StringComparison.Ordinal)
            || string.Equals(code, "DuplicateUserName", StringComparison.Ordinal));

        return new UserAccountCreationResult(
            identityResult.Succeeded,
            conflict,
            errorCodes);
    }

    public async Task<bool> CheckPasswordAsync(
        string userId,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            password,
            lockoutOnFailure: true);
        cancellationToken.ThrowIfCancellationRequested();
        return signInResult.Succeeded;
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Array.Empty<string>();
        }

        var roles = await _userManager.GetRolesAsync(user);
        cancellationToken.ThrowIfCancellationRequested();
        return roles.ToArray();
    }

    public async Task<bool> IsInRoleAsync(
        string userId,
        string role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        bool isInRole = await _userManager.IsInRoleAsync(user, role);
        cancellationToken.ThrowIfCancellationRequested();
        return isInRole;
    }

    public async Task<UserAccountOperationResult> EnsureRoleExistsAsync(
        string role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string roleName = role.Trim();
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            return new UserAccountOperationResult(true, Array.Empty<string>());
        }

        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
        cancellationToken.ThrowIfCancellationRequested();

        if (!result.Succeeded && await _roleManager.RoleExistsAsync(roleName))
        {
            return new UserAccountOperationResult(true, Array.Empty<string>());
        }

        return new UserAccountOperationResult(
            result.Succeeded,
            result.Errors.Select(error => error.Code)
                .Distinct(StringComparer.Ordinal)
                .ToArray());
    }

    public async Task<UserAccountOperationResult> AddToRoleAsync(
        string userId,
        string role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new UserAccountOperationResult(false, new[] { "UserNotFound" });
        }

        var identityResult = await _userManager.AddToRoleAsync(user, role);
        cancellationToken.ThrowIfCancellationRequested();

        return new UserAccountOperationResult(
            identityResult.Succeeded,
            identityResult.Errors
                .Select(error => error.Code)
                .Distinct(StringComparer.Ordinal)
                .ToArray());
    }

    private static UserAccount ToAccount(ApplicationUser user)
    {
        return new UserAccount(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty);
    }
}
