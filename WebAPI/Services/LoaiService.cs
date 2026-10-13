using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs.Common;
using WebAPI.DTOs.Loai;
using WebAPI.Models;
using WebAPI.Services.Interfaces;

namespace WebAPI.Services;

public class LoaiService : ILoaiService
{
    private readonly QLSanPhamDBContext _context;
    private readonly ILogger<LoaiService> _logger;

    public LoaiService( QLSanPhamDBContext context,ILogger<LoaiService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResultDto<LoaiDto>> GetAllAsync( int page,int pageSize,string? sortBy,string? sortOrder)
    {
        ValidatePaging(page, pageSize);

        IQueryable<Loai> query = _context.Loais.AsNoTracking();

        bool descending = string.Equals(
            sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        query = (sortBy ?? "id").ToLowerInvariant() switch
        {
            "ten" => descending? query.OrderByDescending(x => x.Ten): query.OrderBy(x => x.Ten),

            "id" => descending? query.OrderByDescending(x => x.Id): query.OrderBy(x => x.Id),

            _ => throw new ArgumentException("sortBy chỉ hỗ trợ id hoặc ten.")
        };

        int totalItems = await query.CountAsync();

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(x => new LoaiDto
            {
                Id = x.Id,
                Ten = x.Ten,
                Mota = x.Mota
            }).ToListAsync();

        return new PagedResultDto<LoaiDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<LoaiDto> GetByIdAsync(int id)
    {
        var loai = await _context.Loais.AsNoTracking().Where(x => x.Id == id).Select(x => new LoaiDto
            {
                Id = x.Id,
                Ten = x.Ten,
                Mota = x.Mota
            }).FirstOrDefaultAsync();

        return loai ?? throw new KeyNotFoundException(
            $"Không tìm thấy loại có ID {id}.");
    }

    public async Task<LoaiDto> CreateAsync(CreateLoaiDto dto)
    {
        string ten = dto.Ten.Trim();

        bool exists = await _context.Loais.AnyAsync(x => x.Ten == ten);

        if (exists)
        {
            throw new InvalidOperationException(
                "Tên loại đã tồn tại.");
        }

        var loai = new Loai
        {
            Ten = ten,
            Mota = dto.Mota?.Trim()
        };

        _context.Loais.Add(loai);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Đã tạo loại sản phẩm ID {LoaiId}", loai.Id);

        return new LoaiDto
        {
            Id = loai.Id,
            Ten = loai.Ten,
            Mota = loai.Mota
        };
    }

    public async Task<LoaiDto> UpdateAsync(
        int id, UpdateLoaiDto dto)
    {
        var loai = await _context.Loais.FirstOrDefaultAsync(x => x.Id == id);

        if (loai == null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy loại có ID {id}.");
        }

        string ten = dto.Ten.Trim();

        bool duplicate = await _context.Loais.AnyAsync(x => x.Id != id && x.Ten == ten);

        if (duplicate)
        {
            throw new InvalidOperationException(
                "Tên loại đã tồn tại.");
        }

        loai.Ten = ten;
        loai.Mota = dto.Mota?.Trim();

        await _context.SaveChangesAsync();

        return new LoaiDto
        {
            Id = loai.Id,
            Ten = loai.Ten,
            Mota = loai.Mota
        };
    }

    public async Task DeleteAsync(int id)
    {
        var loai = await _context.Loais.FirstOrDefaultAsync(x => x.Id == id);

        if (loai == null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy loại có ID {id}.");
        }

        bool hasProducts = await _context.SanPhams.AnyAsync(x => x.LoaiId == id);

        if (hasProducts)
        {
            throw new InvalidOperationException(
                "Không thể xóa loại đang có sản phẩm.");
        }

        _context.Loais.Remove(loai);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Đã xóa loại sản phẩm ID {LoaiId}", id);
    }

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentException(
                "page phải lớn hơn hoặc bằng 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException(
                "pageSize phải nằm trong khoảng từ 1 đến 100.");
        }
    }
}
