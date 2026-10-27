using QLSanPham.Application.DTOs.Common;
using QLSanPham.Application.DTOs.Loai;
using QLSanPham.Domain.Entities;

namespace QLSanPham.Application.Abstractions.Persistence;

public interface ILoaiRepository
{
    Task<PagedResultDto<LoaiDto>> GetPageAsync(
        int page,
        int pageSize,
        string sortBy,
        bool descending,
        CancellationToken cancellationToken);

    Task<LoaiDto?> GetDtoByIdAsync(int id, CancellationToken cancellationToken);

    Task<Loai?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExistsByNameAsync(
        string name,
        int? excludingId,
        CancellationToken cancellationToken);

    Task<bool> HasProductsAsync(int loaiId, CancellationToken cancellationToken);

    void Add(Loai loai);

    void Remove(Loai loai);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
