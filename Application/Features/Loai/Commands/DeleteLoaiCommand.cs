using MediatR;
using QLSanPham.Application.Abstractions.Messaging;
using QLSanPham.Application.Abstractions.Persistence;

namespace QLSanPham.Application.Features.Loai.Commands;

public sealed record DeleteLoaiCommand(int Id) : IRequest<Unit>, ITransactionalCommand;

public sealed class DeleteLoaiCommandHandler
    : IRequestHandler<DeleteLoaiCommand, Unit>
{
    private readonly ILoaiRepository _repository;

    public DeleteLoaiCommandHandler(ILoaiRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(
        DeleteLoaiCommand request,
        CancellationToken cancellationToken)
    {
        var loai = await _repository.GetTrackedByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Không tìm thấy loại có ID {request.Id}.");

        if (await _repository.HasProductsAsync(request.Id, cancellationToken))
        {
            throw new InvalidOperationException(
                "Không thể xóa loại đang có sản phẩm.");
        }

        _repository.Remove(loai);
        await _repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
