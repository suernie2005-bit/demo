using QLSanPham.Application.DTOs.Common;
using QLSanPham.Application.DTOs.SanPham;

namespace QLSanPham.Application.Services.Interfaces;

public interface ISanPhamService
{
    Task<PagedResultDto<SanPhamDto>> GetAllAsync( int page,int pageSize,string? search,string? sortBy, string? sortOrder,int? loaiId);

    Task<SanPhamDto> GetByIdAsync(int id);

    Task<SanPhamDto> CreateAsync(CreateSanPhamDto dto);

    Task<SanPhamDto> UpdateAsync(int id, UpdateSanPhamDto dto);

    Task DeleteAsync(int id);
}
