using MediatR;
using QLSanPham.Application.Abstractions.Messaging;
using QLSanPham.Application.Abstractions.Identity;
using QLSanPham.Application.DTOs.Auth;

namespace QLSanPham.Application.Features.Auth.Commands;

public sealed record RegisterAccountCommand(string Email, string Password)
    : IRequest<RegisteredAccountDto>, ITransactionalCommand;

public sealed class RegisterAccountCommandHandler
    : IRequestHandler<RegisterAccountCommand, RegisteredAccountDto>
{
    private readonly IUserAccountService _userAccountService;

    public RegisterAccountCommandHandler(IUserAccountService userAccountService)
    {
        _userAccountService = userAccountService;
    }

    public async Task<RegisteredAccountDto> Handle(
        RegisterAccountCommand request,
        CancellationToken cancellationToken)
    {
        // Roles are created as definitions only. Public registration always assigns User.
        foreach (string role in new[] { RoleNames.User, RoleNames.Admin })
        {
            var roleResult = await _userAccountService.EnsureRoleExistsAsync(
                role,
                cancellationToken);

            if (!roleResult.Succeeded)
            {
                throw new InvalidDataException("Không thể khởi tạo vai trò tài khoản.");
            }
        }

        var createResult = await _userAccountService.CreateAsync(
            request.Email.Trim(),
            request.Password,
            cancellationToken);

        if (!createResult.Succeeded)
        {
            if (createResult.Conflict)
            {
                throw new InvalidOperationException("Email đã được đăng ký.");
            }

            throw new ArgumentException("Thông tin đăng ký không hợp lệ.");
        }

        var account = await _userAccountService.FindByEmailAsync(
            request.Email.Trim(),
            cancellationToken);

        if (account is null)
        {
            throw new InvalidDataException("Không thể đọc tài khoản vừa tạo.");
        }

        var assignResult = await _userAccountService.AddToRoleAsync(
            account.Id,
            RoleNames.User,
            cancellationToken);

        if (!assignResult.Succeeded)
        {
            throw new InvalidDataException("Không thể gán vai trò mặc định cho tài khoản.");
        }

        return new RegisteredAccountDto(account.Id, account.Email);
    }
}
