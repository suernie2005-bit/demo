using Microsoft.Extensions.Logging;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.Common;
using QLSanPham.Application.DTOs.Loai;
using QLSanPham.Application.Features.Loai.Commands;
using QLSanPham.Application.Features.Loai.Queries;
using QLSanPham.Application.Features.Loai.Validation;
using QLSanPham.Application.Services.Interfaces;
using QLSanPham.Infrastructure.Persistence;
using QLSanPham.Infrastructure.Persistence.Repositories;

namespace QLSanPham.Infrastructure.Services;

public class LoaiService : ILoaiService
{
    private readonly ILoaiRepository _repository;
    private readonly ILogger<LoaiService> _logger;

    public LoaiService( QLSanPhamDBContext context,ILogger<LoaiService> logger)
    {
        _repository = new LoaiRepository(context);
        _logger = logger;
    }

    public async Task<PagedResultDto<LoaiDto>> GetAllAsync(
        int page,
        int pageSize,
        string? sortBy,
        string? sortOrder)
    {
        var query = new GetLoaiListQuery(page, pageSize, sortBy, sortOrder);
        LegacyServiceValidation.EnsureValid(query, new GetLoaiListQueryValidator());
        return await new GetLoaiListQueryHandler(_repository)
            .Handle(query, CancellationToken.None);
    }

    public async Task<LoaiDto> GetByIdAsync(int id)
    {
        var query = new GetLoaiByIdQuery(id);
        LegacyServiceValidation.EnsureValid(query, new GetLoaiByIdQueryValidator());
        return await new GetLoaiByIdQueryHandler(_repository)
            .Handle(query, CancellationToken.None);
    }

    public async Task<LoaiDto> CreateAsync(CreateLoaiDto dto)
    {
        var command = new CreateLoaiCommand(dto);
        LegacyServiceValidation.EnsureValid(command, new CreateLoaiCommandValidator());
        var result = await new CreateLoaiCommandHandler(_repository)
            .Handle(command, CancellationToken.None);

        _logger.LogInformation("Đã tạo loại sản phẩm ID {LoaiId}", result.Id);
        return result;
    }

    public async Task<LoaiDto> UpdateAsync(
        int id, UpdateLoaiDto dto)
    {
        var command = new UpdateLoaiCommand(id, dto);
        LegacyServiceValidation.EnsureValid(command, new UpdateLoaiCommandValidator());
        return await new UpdateLoaiCommandHandler(_repository)
            .Handle(command, CancellationToken.None);
    }

    public async Task DeleteAsync(int id)
    {
        var command = new DeleteLoaiCommand(id);
        LegacyServiceValidation.EnsureValid(command, new DeleteLoaiCommandValidator());
        await new DeleteLoaiCommandHandler(_repository)
            .Handle(command, CancellationToken.None);

        _logger.LogInformation("Đã xóa loại sản phẩm ID {LoaiId}", id);
    }

}
