using MediatR;
using QLSanPham.Application.Abstractions.Messaging;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.Loai;

namespace QLSanPham.Application.Features.Loai.Commands;

public sealed record UpdateLoaiCommand(int Id, UpdateLoaiDto Data) : IRequest<LoaiDto>, ITransactionalCommand;

public sealed class UpdateLoaiCommandHandler
    : IRequestHandler<UpdateLoaiCommand, LoaiDto>
{
    private readonly ILoaiRepository _repository;

    public UpdateLoaiCommandHandler(ILoaiRepository repository)
    {
        _repository = repository;
    }

    public async Task<LoaiDto> Handle(
        UpdateLoaiCommand request,
        CancellationToken cancellationToken)
    {
        var loai = await _repository.GetTrackedByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Không tìm thấy loại có ID {request.Id}.");

        string name = request.Data.Ten.Trim();

        if (await _repository.ExistsByNameAsync(name, request.Id, cancellationToken))
        {
            throw new InvalidOperationException("Tên loại đã tồn tại.");
        }

        loai.Ten = name;
        loai.Mota = request.Data.Mota?.Trim();

        await _repository.SaveChangesAsync(cancellationToken);

        return new LoaiDto
        {
            Id = loai.Id,
            Ten = loai.Ten,
            Mota = loai.Mota
        };
    }
}
