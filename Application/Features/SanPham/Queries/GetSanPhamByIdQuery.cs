using MediatR;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.SanPham;

namespace QLSanPham.Application.Features.SanPham.Queries;

public sealed record GetSanPhamByIdQuery(int Id) : IRequest<SanPhamDto>;

public sealed class GetSanPhamByIdQueryHandler
    : IRequestHandler<GetSanPhamByIdQuery, SanPhamDto>
{
    private readonly ISanPhamRepository _repository;

    public GetSanPhamByIdQueryHandler(ISanPhamRepository repository)
    {
        _repository = repository;
    }

    public async Task<SanPhamDto> Handle(
        GetSanPhamByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _repository.GetDtoByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Không tìm thấy sản phẩm có ID {request.Id}.");
    }
}
