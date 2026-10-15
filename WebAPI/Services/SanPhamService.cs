using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs.Common;
using WebAPI.DTOs.SanPham;
using WebAPI.Exceptions;
using WebAPI.Models;
using WebAPI.Services.Interfaces;

namespace WebAPI.Services;

public class SanPhamService : ISanPhamService
{
    private readonly QLSanPhamDBContext _context;
    private readonly ILogger<SanPhamService> _logger;

    public SanPhamService(QLSanPhamDBContext context,ILogger<SanPhamService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResultDto<SanPhamDto>> GetAllAsync(int page,int pageSize,string? search,string? sortBy, string? sortOrder, int? loaiId)
    {
        ValidatePaging(page, pageSize);

        IQueryable<SanPham> query =_context.SanPhams.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string keyword = search.Trim();

            query = query.Where(x => x.TenSp.Contains(keyword));
        }

        if (loaiId.HasValue)
        {
            query = query.Where(x => x.LoaiId == loaiId.Value);
        }

        bool descending = string.Equals(sortOrder, "desc",StringComparison.OrdinalIgnoreCase);

        query = (sortBy ?? "id").ToLowerInvariant() switch
        {
            "id" => descending ? query.OrderByDescending(x => x.Id): query.OrderBy(x => x.Id),

            "tensp" => descending ? query.OrderByDescending(x => x.TenSp): query.OrderBy(x => x.TenSp),

            "gia" => descending ? query.OrderByDescending(x => x.Gia): query.OrderBy(x => x.Gia),

            "soluong" => descending? query.OrderByDescending(x => x.SoLuong): query.OrderBy(x => x.SoLuong),

            "ngay" => descending? query.OrderByDescending(x => x.Ngay): query.OrderBy(x => x.Ngay),

            _ => throw new ArgumentException("sortBy chỉ hỗ trợ id, tenSP, gia, soLuong hoặc ngay.")
        };

        int totalItems = await query.CountAsync();

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(x => new SanPhamDto
            {
                Id = x.Id,
                TenSP = x.TenSp,
                MoTaSP = x.MoTaSp,
                Gia = x.Gia,
                SoLuong = x.SoLuong,
                LoaiId = x.LoaiId,
                TenLoai = x.Loai.Ten,
                Ngay = x.Ngay
            }).ToListAsync();

        return new PagedResultDto<SanPhamDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<SanPhamDto> GetByIdAsync(int id)
    {
        var sanPham = await _context.SanPhams.AsNoTracking().Where(x => x.Id == id).Select(x => new SanPhamDto
            {
                Id = x.Id,
                TenSP = x.TenSp,
                MoTaSP = x.MoTaSp,
                Gia = x.Gia,
                SoLuong = x.SoLuong,
                LoaiId = x.LoaiId,
                TenLoai = x.Loai.Ten,
                Ngay = x.Ngay
            }).FirstOrDefaultAsync();

        return sanPham ?? throw new KeyNotFoundException( $"Không tìm thấy sản phẩm có ID {id}.");
    }

    public async Task<SanPhamDto> CreateAsync(
        CreateSanPhamDto dto)
    {
        string ten = dto.TenSP.Trim();

        bool loaiExists = await _context.Loais.AnyAsync(x => x.Id == dto.LoaiId);

        if (!loaiExists)
        {
            throw new ArgumentException("Loại sản phẩm được chọn không tồn tại.");
        }

        bool duplicate = await _context.SanPhams.AnyAsync(x => x.TenSp == ten && x.LoaiId == dto.LoaiId);

        if (duplicate)
        {
            throw new ConflictException("Sản phẩm cùng tên trong loại này đã tồn tại.");
        }

        var sanPham = new SanPham
        {
            TenSp = ten,
            MoTaSp = dto.MoTaSP?.Trim(),
            Gia = dto.Gia,
            SoLuong = dto.SoLuong,
            LoaiId = dto.LoaiId,
            Ngay = DateTime.UtcNow
        };

        _context.SanPhams.Add(sanPham);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã tạo sản phẩm ID {SanPhamId}", sanPham.Id);

        return await GetByIdAsync(sanPham.Id);
    }

    public async Task<SanPhamDto> UpdateAsync(
        int id, UpdateSanPhamDto dto)
    {
        var sanPham = await _context.SanPhams.FirstOrDefaultAsync(x => x.Id == id);

        if (sanPham == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy sản phẩm có ID {id}.");
        }

        bool loaiExists = await _context.Loais.AnyAsync(x => x.Id == dto.LoaiId);

        if (!loaiExists)
        {
            throw new ArgumentException("Loại sản phẩm được chọn không tồn tại.");
        }

        string ten = dto.TenSP.Trim();

        bool duplicate = await _context.SanPhams.AnyAsync(x =>x.Id != id && x.TenSp == ten && x.LoaiId == dto.LoaiId);

        if (duplicate)
        {
            throw new ConflictException("Sản phẩm cùng tên trong loại này đã tồn tại.");
        }

        sanPham.TenSp = ten;
        sanPham.MoTaSp = dto.MoTaSP?.Trim();
        sanPham.Gia = dto.Gia;
        sanPham.SoLuong = dto.SoLuong;
        sanPham.LoaiId = dto.LoaiId;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
    {
        var sanPham = await _context.SanPhams.FirstOrDefaultAsync(x => x.Id == id);

        if (sanPham == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy sản phẩm có ID {id}.");
        }

        _context.SanPhams.Remove(sanPham);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Đã xóa sản phẩm ID {SanPhamId}", id);
    }

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentException("page phải lớn hơn hoặc bằng 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException("pageSize phải nằm trong khoảng từ 1 đến 100.");
        }
    }
}
