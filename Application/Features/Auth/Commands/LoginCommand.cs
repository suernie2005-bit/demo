using MediatR;
using QLSanPham.Application.Abstractions.Identity;
using QLSanPham.Application.Abstractions.Messaging;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.Auth;
using QLSanPham.Domain.Entities;

namespace QLSanPham.Application.Features.Auth.Commands;

public sealed record LoginCommand(string Email, string Password)
    : IRequest<AuthenticationResult>, ITransactionalCommand;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthenticationResult>
{
    private readonly IUserAccountService _userAccountService;
    private readonly IAuthenticationTokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LoginCommandHandler(
        IUserAccountService userAccountService,
        IAuthenticationTokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userAccountService = userAccountService;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<AuthenticationResult> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var account = await _userAccountService.FindByEmailAsync(
            request.Email.Trim(),
            cancellationToken);

        if (account is null || !await _userAccountService.CheckPasswordAsync(
                account.Id,
                request.Password,
                cancellationToken))
        {
            return AuthenticationResult.InvalidCredentials;
        }

        var roles = await _userAccountService.GetRolesAsync(account.Id, cancellationToken);
        Guid familyId = Guid.NewGuid();
        var issue = _tokenService.Issue(account, roles, familyId);

        _refreshTokenRepository.Add(new RefreshToken
        {
            UserId = account.Id,
            TokenHash = issue.RefreshTokenHash,
            FamilyId = familyId,
            CreatedAtUtc = issue.RefreshTokenCreatedAtUtc,
            ExpiresAtUtc = issue.Response.RefreshTokenExpiresAtUtc
        });

        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
        return AuthenticationResult.Success(issue.Response);
    }
}
