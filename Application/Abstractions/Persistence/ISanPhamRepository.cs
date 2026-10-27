using QLSanPham.Application.DTOs.Common;
using QLSanPham.Application.DTOs.SanPham;
using QLSanPham.Domain.Entities;

namespace QLSanPham.Application.Abstractions.Persistence;

public interface ISanPhamRepository
{
    Task<PagedResultDto<SanPhamDto>> GetPageAsync(
        int page,
        int pageSize,
        string? search,
        int? loaiId,
        string sortBy,
        bool descending,
        CancellationToken cancellationToken);

    Task<SanPhamDto?> GetDtoByIdAsync(int id, CancellationToken cancellationToken);

    Task<SanPham?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> CategoryExistsAsync(int loaiId, CancellationToken cancellationToken);

    Task<bool> ExistsByNameInCategoryAsync(
        string name,
        int loaiId,
        int? excludingId,
        CancellationToken cancellationToken);

    void Add(SanPham sanPham);

    void Remove(SanPham sanPham);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
