using Microsoft.EntityFrameworkCore;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.Common;
using QLSanPham.Application.DTOs.Loai;
using QLSanPham.Domain.Entities;

namespace QLSanPham.Infrastructure.Persistence.Repositories;

public sealed class LoaiRepository : ILoaiRepository
{
    private readonly QLSanPhamDBContext _context;

    public LoaiRepository(QLSanPhamDBContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<LoaiDto>> GetPageAsync(
        int page,
        int pageSize,
        string sortBy,
        bool descending,
        CancellationToken cancellationToken)
    {
        IQueryable<Loai> query = _context.Loais.AsNoTracking();

        query = sortBy switch
        {
            "ten" => descending
                ? query.OrderByDescending(x => x.Ten)
                : query.OrderBy(x => x.Ten),
            _ => descending
                ? query.OrderByDescending(x => x.Id)
                : query.OrderBy(x => x.Id)
        };

        int totalItems = await query.CountAsync(cancellationToken);
        long offset = ((long)page - 1) * pageSize;

        var items = offset > int.MaxValue
            ? new List<LoaiDto>()
            : await query.Skip((int)offset)
                .Take(pageSize)
                .Select(x => new LoaiDto
                {
                    Id = x.Id,
                    Ten = x.Ten,
                    Mota = x.Mota
                })
                .ToListAsync(cancellationToken);

        return new PagedResultDto<LoaiDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public Task<LoaiDto?> GetDtoByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _context.Loais
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new LoaiDto
            {
                Id = x.Id,
                Ten = x.Ten,
                Mota = x.Mota
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Loai?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _context.Loais.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(
        string name,
        int? excludingId,
        CancellationToken cancellationToken)
    {
        IQueryable<Loai> query = _context.Loais.Where(x => x.Ten == name);

        if (excludingId.HasValue)
        {
            query = query.Where(x => x.Id != excludingId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> HasProductsAsync(int loaiId, CancellationToken cancellationToken)
    {
        return _context.SanPhams.AnyAsync(x => x.LoaiId == loaiId, cancellationToken);
    }

    public void Add(Loai loai)
    {
        _context.Loais.Add(loai);
    }

    public void Remove(Loai loai)
    {
        _context.Loais.Remove(loai);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
