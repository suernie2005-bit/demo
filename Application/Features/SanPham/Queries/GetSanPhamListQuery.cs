using MediatR;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.Common;
using QLSanPham.Application.DTOs.SanPham;

namespace QLSanPham.Application.Features.SanPham.Queries;

public sealed record GetSanPhamListQuery(
    int Page,
    int PageSize,
    string? Search,
    string? SortBy,
    string? SortOrder,
    int? LoaiId) : IRequest<PagedResultDto<SanPhamDto>>;

public sealed class GetSanPhamListQueryHandler
    : IRequestHandler<GetSanPhamListQuery, PagedResultDto<SanPhamDto>>
{
    private readonly ISanPhamRepository _repository;

    public GetSanPhamListQueryHandler(ISanPhamRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResultDto<SanPhamDto>> Handle(
        GetSanPhamListQuery request,
        CancellationToken cancellationToken)
    {
        bool descending = string.Equals(
            request.SortOrder?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);
        string sortBy = (request.SortBy ?? "id").Trim().ToLowerInvariant();

        return _repository.GetPageAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.LoaiId,
            sortBy,
            descending,
            cancellationToken);
    }
}
