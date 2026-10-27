using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.Common;
using QLSanPham.Application.DTOs.SanPham;
using QLSanPham.Application.Features.SanPham.Commands;
using QLSanPham.Application.Features.SanPham.Queries;
using QLSanPham.Application.Features.SanPham.Validation;
using QLSanPham.Application.Services.Interfaces;
using QLSanPham.Domain.Entities;
using QLSanPham.Infrastructure.Persistence;
using QLSanPham.Infrastructure.Persistence.Repositories;

namespace QLSanPham.Infrastructure.Services;

public class SanPhamService : ISanPhamService
{
    private readonly ISanPhamRepository _repository;
    private readonly ILogger<SanPhamService> _logger;

    public SanPhamService(QLSanPhamDBContext context,ILogger<SanPhamService> logger)
    {
        _repository = new SanPhamRepository(context);
        _logger = logger;
    }

    public async Task<PagedResultDto<SanPhamDto>> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        string? sortOrder,
        int? loaiId)
    {
        var query = new GetSanPhamListQuery(
            page,
            pageSize,
            search,
            sortBy,
            sortOrder,
            loaiId);
        LegacyServiceValidation.EnsureValid(query, new GetSanPhamListQueryValidator());

        return await new GetSanPhamListQueryHandler(_repository)
            .Handle(query, CancellationToken.None);
    }

    public Task<SanPhamDto> GetByIdAsync(int id)
    {
        var query = new GetSanPhamByIdQuery(id);
        LegacyServiceValidation.EnsureValid(query, new GetSanPhamByIdQueryValidator());
        return new GetSanPhamByIdQueryHandler(_repository)
            .Handle(query, CancellationToken.None);
    }

    public async Task<SanPhamDto> CreateAsync(CreateSanPhamDto dto)
    {
        var command = new CreateSanPhamCommand(dto);
        LegacyServiceValidation.EnsureValid(command, new CreateSanPhamCommandValidator());
        var result = await new CreateSanPhamCommandHandler(_repository)
            .Handle(command, CancellationToken.None);

        _logger.LogInformation("Đã tạo sản phẩm ID {SanPhamId}", result.Id);
        return result;
    }

    public Task<SanPhamDto> UpdateAsync(int id, UpdateSanPhamDto dto)
    {
        var command = new UpdateSanPhamCommand(id, dto);
        LegacyServiceValidation.EnsureValid(command, new UpdateSanPhamCommandValidator());
        return new UpdateSanPhamCommandHandler(_repository)
            .Handle(command, CancellationToken.None);
    }

    public async Task DeleteAsync(int id)
    {
        var command = new DeleteSanPhamCommand(id);
        LegacyServiceValidation.EnsureValid(command, new DeleteSanPhamCommandValidator());
        await new DeleteSanPhamCommandHandler(_repository)
            .Handle(command, CancellationToken.None);
        _logger.LogInformation("Đã xóa sản phẩm ID {SanPhamId}", id);
    }

}
