using MediatR;
using QLSanPham.Application.Abstractions.Messaging;
using QLSanPham.Application.Abstractions.Persistence;

namespace QLSanPham.Application.Features.SanPham.Commands;

public sealed record DeleteSanPhamCommand(int Id) : IRequest<Unit>, ITransactionalCommand;

public sealed class DeleteSanPhamCommandHandler
    : IRequestHandler<DeleteSanPhamCommand, Unit>
{
    private readonly ISanPhamRepository _repository;

    public DeleteSanPhamCommandHandler(ISanPhamRepository repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(
        DeleteSanPhamCommand request,
        CancellationToken cancellationToken)
    {
        var sanPham = await _repository.GetTrackedByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Không tìm thấy sản phẩm có ID {request.Id}.");

        _repository.Remove(sanPham);
        await _repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
