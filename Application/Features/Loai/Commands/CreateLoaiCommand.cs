using MediatR;
using QLSanPham.Application.Abstractions.Messaging;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.Loai;
using LoaiEntity = QLSanPham.Domain.Entities.Loai;

namespace QLSanPham.Application.Features.Loai.Commands;

public sealed record CreateLoaiCommand(CreateLoaiDto Data) : IRequest<LoaiDto>, ITransactionalCommand;

public sealed class CreateLoaiCommandHandler
    : IRequestHandler<CreateLoaiCommand, LoaiDto>
{
    private readonly ILoaiRepository _repository;

    public CreateLoaiCommandHandler(ILoaiRepository repository)
    {
        _repository = repository;
    }

    public async Task<LoaiDto> Handle(
        CreateLoaiCommand request,
        CancellationToken cancellationToken)
    {
        string name = request.Data.Ten.Trim();

        if (await _repository.ExistsByNameAsync(name, null, cancellationToken))
        {
            throw new InvalidOperationException("Tên loại đã tồn tại.");
        }

        var loai = new LoaiEntity
        {
            Ten = name,
            Mota = request.Data.Mota?.Trim()
        };

        _repository.Add(loai);
        await _repository.SaveChangesAsync(cancellationToken);

        return new LoaiDto
        {
            Id = loai.Id,
            Ten = loai.Ten,
            Mota = loai.Mota
        };
    }
}
