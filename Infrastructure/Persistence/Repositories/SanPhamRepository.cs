using Microsoft.EntityFrameworkCore;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.Common;
using QLSanPham.Application.DTOs.SanPham;
using QLSanPham.Domain.Entities;

namespace QLSanPham.Infrastructure.Persistence.Repositories;

public sealed class SanPhamRepository : ISanPhamRepository
{
    private readonly QLSanPhamDBContext _context;

    public SanPhamRepository(QLSanPhamDBContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<SanPhamDto>> GetPageAsync(
        int page,
        int pageSize,
        string? search,
        int? loaiId,
        string sortBy,
        bool descending,
        CancellationToken cancellationToken)
    {
        IQueryable<SanPham> query = _context.SanPhams.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string keyword = search.Trim();
            query = query.Where(x => x.TenSp.Contains(keyword));
        }

        if (loaiId.HasValue)
        {
            query = query.Where(x => x.LoaiId == loaiId.Value);
        }

        query = sortBy switch
        {
            "tensp" => descending
                ? query.OrderByDescending(x => x.TenSp)
                : query.OrderBy(x => x.TenSp),
            "gia" => descending
                ? query.OrderByDescending(x => x.Gia)
                : query.OrderBy(x => x.Gia),
            "soluong" => descending
                ? query.OrderByDescending(x => x.SoLuong)
                : query.OrderBy(x => x.SoLuong),
            "ngay" => descending
                ? query.OrderByDescending(x => x.Ngay)
                : query.OrderBy(x => x.Ngay),
            _ => descending
                ? query.OrderByDescending(x => x.Id)
                : query.OrderBy(x => x.Id)
        };

        int totalItems = await query.CountAsync(cancellationToken);
        long offset = ((long)page - 1) * pageSize;

        var items = offset > int.MaxValue
            ? new List<SanPhamDto>()
            : await query.Skip((int)offset)
                .Take(pageSize)
                .Select(x => new SanPhamDto
                {
                    Id = x.Id,
                    TenSP = x.TenSp,
                    MoTaSP = x.MoTaSp,
                    Gia = x.Gia,
                    SoLuong = x.SoLuong,
                    LoaiId = x.LoaiId,
                    TenLoai = x.Loai.Ten,
                    Ngay = x.Ngay
                })
                .ToListAsync(cancellationToken);

        return new PagedResultDto<SanPhamDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public Task<SanPhamDto?> GetDtoByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _context.SanPhams
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SanPhamDto
            {
                Id = x.Id,
                TenSP = x.TenSp,
                MoTaSP = x.MoTaSp,
                Gia = x.Gia,
                SoLuong = x.SoLuong,
                LoaiId = x.LoaiId,
                TenLoai = x.Loai.Ten,
                Ngay = x.Ngay
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<SanPham?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _context.SanPhams.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> CategoryExistsAsync(int loaiId, CancellationToken cancellationToken)
    {
        return _context.Loais.AnyAsync(x => x.Id == loaiId, cancellationToken);
    }

    public Task<bool> ExistsByNameInCategoryAsync(
        string name,
        int loaiId,
        int? excludingId,
        CancellationToken cancellationToken)
    {
        IQueryable<SanPham> query = _context.SanPhams
            .Where(x => x.TenSp == name && x.LoaiId == loaiId);

        if (excludingId.HasValue)
        {
            query = query.Where(x => x.Id != excludingId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public void Add(SanPham sanPham)
    {
        _context.SanPhams.Add(sanPham);
    }

    public void Remove(SanPham sanPham)
    {
        _context.SanPhams.Remove(sanPham);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
