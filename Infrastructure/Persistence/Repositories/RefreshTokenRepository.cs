using Microsoft.EntityFrameworkCore;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Domain.Entities;

namespace QLSanPham.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly QLSanPhamDBContext _dbContext;

    public RefreshTokenRepository(QLSanPhamDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<RefreshToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        return _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                refreshToken => refreshToken.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetByFamilyIdAsync(
        Guid familyId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.RefreshTokens
            .Where(refreshToken => refreshToken.FamilyId == familyId)
            .OrderBy(refreshToken => refreshToken.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public void Add(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Add(refreshToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
