using QLSanPham.Domain.Entities;

namespace QLSanPham.Application.Abstractions.Persistence;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task<IReadOnlyList<RefreshToken>> GetByFamilyIdAsync(
        Guid familyId,
        CancellationToken cancellationToken);

    void Add(RefreshToken refreshToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
