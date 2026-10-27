using MediatR;
using QLSanPham.Application.Abstractions.Identity;
using QLSanPham.Application.Abstractions.Messaging;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.Auth;
using QLSanPham.Domain.Entities;

namespace QLSanPham.Application.Features.Auth.Commands;

public sealed record RefreshTokenCommand(string RefreshToken)
    : IRequest<AuthenticationResult>, ITransactionalCommand;

public sealed class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, AuthenticationResult>
{
    private readonly IUserAccountService _userAccountService;
    private readonly IAuthenticationTokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly TimeProvider _timeProvider;

    public RefreshTokenCommandHandler(
        IUserAccountService userAccountService,
        IAuthenticationTokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        TimeProvider timeProvider)
    {
        _userAccountService = userAccountService;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _timeProvider = timeProvider;
    }

    public async Task<AuthenticationResult> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        string tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var current = await _refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);

        if (current is null)
        {
            return AuthenticationResult.InvalidCredentials;
        }

        if (current.RevokedAtUtc is not null)
        {
            await RevokeFamilyAsync(current.FamilyId, cancellationToken);
            return AuthenticationResult.InvalidCredentials;
        }

        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        if (current.ExpiresAtUtc <= now)
        {
            return AuthenticationResult.InvalidCredentials;
        }

        var account = await _userAccountService.FindByIdAsync(current.UserId, cancellationToken);
        if (account is null)
        {
            await RevokeFamilyAsync(current.FamilyId, cancellationToken);
            return AuthenticationResult.InvalidCredentials;
        }

        var roles = await _userAccountService.GetRolesAsync(account.Id, cancellationToken);
        var issue = _tokenService.Issue(account, roles, current.FamilyId);
        var replacement = new RefreshToken
        {
            UserId = current.UserId,
            TokenHash = issue.RefreshTokenHash,
            FamilyId = current.FamilyId,
            CreatedAtUtc = issue.RefreshTokenCreatedAtUtc,
            ExpiresAtUtc = issue.Response.RefreshTokenExpiresAtUtc
        };

        current.RevokedAtUtc = now;
        current.ReplacedByTokenId = replacement.Id;
        current.ReplacedByToken = replacement;
        _refreshTokenRepository.Add(replacement);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return AuthenticationResult.Success(issue.Response);
    }

    private async Task RevokeFamilyAsync(Guid familyId, CancellationToken cancellationToken)
    {
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        var family = await _refreshTokenRepository.GetByFamilyIdAsync(familyId, cancellationToken);

        foreach (var token in family.Where(token => token.RevokedAtUtc is null))
        {
            token.RevokedAtUtc = now;
        }

        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }
}
