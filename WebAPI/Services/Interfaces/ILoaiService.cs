using WebAPI.DTOs.Common;
using WebAPI.DTOs.Loai;

namespace WebAPI.Services.Interfaces;

public interface ILoaiService
{
    Task<PagedResultDto<LoaiDto>> GetAllAsync(int page, int pageSize, string? sortBy, string? sortOrder);

    Task<LoaiDto> GetByIdAsync(int id);

    Task<LoaiDto> CreateAsync(CreateLoaiDto dto);

    Task<LoaiDto> UpdateAsync(int id, UpdateLoaiDto dto);

    Task DeleteAsync(int id);
}