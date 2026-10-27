using MediatR;
using QLSanPham.Application.Abstractions.Messaging;
using QLSanPham.Application.Abstractions.Persistence;
using QLSanPham.Application.DTOs.SanPham;
using SanPhamEntity = QLSanPham.Domain.Entities.SanPham;

namespace QLSanPham.Application.Features.SanPham.Commands;

public sealed record CreateSanPhamCommand(CreateSanPhamDto Data) : IRequest<SanPhamDto>, ITransactionalCommand;

public sealed class CreateSanPhamCommandHandler
    : IRequestHandler<CreateSanPhamCommand, SanPhamDto>
{
    private readonly ISanPhamRepository _repository;

    public CreateSanPhamCommandHandler(ISanPhamRepository repository)
    {
        _repository = repository;
    }

    public async Task<SanPhamDto> Handle(
        CreateSanPhamCommand request,
        CancellationToken cancellationToken)
    {
        string name = request.Data.TenSP.Trim();

        if (!await _repository.CategoryExistsAsync(request.Data.LoaiId, cancellationToken))
        {
            throw new ArgumentException("Loại sản phẩm được chọn không tồn tại.");
        }

        if (await _repository.ExistsByNameInCategoryAsync(
                name,
                request.Data.LoaiId,
                null,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "Sản phẩm cùng tên trong loại này đã tồn tại.");
        }

        var sanPham = new SanPhamEntity
        {
            TenSp = name,
            MoTaSp = request.Data.MoTaSP?.Trim(),
            Gia = request.Data.Gia,
            SoLuong = request.Data.SoLuong,
            LoaiId = request.Data.LoaiId,
            Ngay = DateTime.UtcNow
        };

        _repository.Add(sanPham);
        await _repository.SaveChangesAsync(cancellationToken);

        return await _repository.GetDtoByIdAsync(sanPham.Id, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Không tìm thấy sản phẩm có ID {sanPham.Id}.");
    }
}
