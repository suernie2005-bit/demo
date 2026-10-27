using MediatR;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.Common;
using QLSanPham.Application.DTOs.Loai;

namespace QLSanPham.Application.Features.Loai.Queries;

public sealed record GetLoaiListQuery(
    int Page,
    int PageSize,
    string? SortBy,
    string? SortOrder) : IRequest<PagedResultDto<LoaiDto>>;

public sealed class GetLoaiListQueryHandler
    : IRequestHandler<GetLoaiListQuery, PagedResultDto<LoaiDto>>
{
    private readonly ILoaiRepository _repository;

    public GetLoaiListQueryHandler(ILoaiRepository repository)
    {
        _repository = repository;
    }

    public Task<PagedResultDto<LoaiDto>> Handle(
        GetLoaiListQuery request,
        CancellationToken cancellationToken)
    {
        bool descending = string.Equals(
            request.SortOrder?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);
        string sortBy = (request.SortBy ?? "id").Trim().ToLowerInvariant();

        return _repository.GetPageAsync(
            request.Page,
            request.PageSize,
            sortBy,
            descending,
            cancellationToken);
    }
}
