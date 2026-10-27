using MediatR;
using QLSanPham.Application.Abstractions.Messaging;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.SanPham;

namespace QLSanPham.Application.Features.SanPham.Commands;

public sealed record UpdateSanPhamCommand(int Id, UpdateSanPhamDto Data) : IRequest<SanPhamDto>, ITransactionalCommand;

public sealed class UpdateSanPhamCommandHandler
    : IRequestHandler<UpdateSanPhamCommand, SanPhamDto>
{
    private readonly ISanPhamRepository _repository;

    public UpdateSanPhamCommandHandler(ISanPhamRepository repository)
    {
        _repository = repository;
    }

    public async Task<SanPhamDto> Handle(
        UpdateSanPhamCommand request,
        CancellationToken cancellationToken)
    {
        var sanPham = await _repository.GetTrackedByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Không tìm thấy sản phẩm có ID {request.Id}.");

        if (!await _repository.CategoryExistsAsync(request.Data.LoaiId, cancellationToken))
        {
            throw new ArgumentException("Loại sản phẩm được chọn không tồn tại.");
        }

        string name = request.Data.TenSP.Trim();

        if (await _repository.ExistsByNameInCategoryAsync(
                name,
                request.Data.LoaiId,
                request.Id,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Sản phẩm cùng tên trong loại này đã tồn tại.");
        }

        sanPham.TenSp = name;
        sanPham.MoTaSp = request.Data.MoTaSP?.Trim();
        sanPham.Gia = request.Data.Gia;
        sanPham.SoLuong = request.Data.SoLuong;
        sanPham.LoaiId = request.Data.LoaiId;

        await _repository.SaveChangesAsync(cancellationToken);

        return await _repository.GetDtoByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Không tìm thấy sản phẩm có ID {request.Id}.");
    }
}
