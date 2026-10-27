using MediatR;
using QLSanPham.Application.Abstractions.Identity;
using QLSanPham.Application.Abstractions.Messaging;
using QLSanPham.Application.Abstractions.Persistence;

namespace QLSanPham.Application.Features.Auth.Commands;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Unit>, ITransactionalCommand;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IAuthenticationTokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly TimeProvider _timeProvider;

    public LogoutCommandHandler(
        IAuthenticationTokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        TimeProvider timeProvider)
    {
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _refreshTokenRepository.GetByHashAsync(
            _tokenService.HashRefreshToken(request.RefreshToken),
            cancellationToken);

        if (token is not null)
        {
            var family = await _refreshTokenRepository.GetByFamilyIdAsync(
                token.FamilyId,
                cancellationToken);
            DateTime now = _timeProvider.GetUtcNow().UtcDateTime;

            foreach (var familyToken in family.Where(token => token.RevokedAtUtc is null))
            {
                familyToken.RevokedAtUtc = now;
            }

            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
        }

        // Idempotent response: do not disclose whether the supplied token existed.
        return Unit.Value;
    }
}
