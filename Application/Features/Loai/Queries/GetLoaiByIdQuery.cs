using MediatR;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.Loai;

namespace QLSanPham.Application.Features.Loai.Queries;

public sealed record GetLoaiByIdQuery(int Id) : IRequest<LoaiDto>;

public sealed class GetLoaiByIdQueryHandler
    : IRequestHandler<GetLoaiByIdQuery, LoaiDto>
{
    private readonly ILoaiRepository _repository;

    public GetLoaiByIdQueryHandler(ILoaiRepository repository)
    {
        _repository = repository;
    }

    public async Task<LoaiDto> Handle(
        GetLoaiByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _repository.GetDtoByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Không tìm thấy loại có ID {request.Id}.");
    }
}
