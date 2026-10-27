using QLSanPham.Application.DTOs.Loai;
using QLSanPham.Application.DTOs.SanPham;
using QLSanPham.Application.Features.Loai.Commands;
using QLSanPham.Application.Features.Loai.Queries;
using QLSanPham.Application.Features.SanPham.Commands;
using QLSanPham.Application.Features.SanPham.Queries;
using QLSanPham.Infrastructure.Persistence.Repositories;
using WebAPI.Tests.TestSupport;

namespace WebAPI.Tests;

public sealed class CqrsCatalogHandlerTests
{
    [Fact]
    public async Task Create_loai_trims_input_and_maps_saved_entity_to_dto()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var handler = new CreateLoaiCommandHandler(new LoaiRepository(context));

        var result = await handler.Handle(
            new CreateLoaiCommand(new CreateLoaiDto { Ten = "  Đồ mới  ", Mota = "  mô tả  " }),
            CancellationToken.None);

        Assert.True(result.Id > 0);
        Assert.Equal("Đồ mới", result.Ten);
        Assert.Equal("mô tả", result.Mota);
    }

    [Fact]
    public async Task Create_loai_rejects_duplicate_name()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var handler = new CreateLoaiCommandHandler(new LoaiRepository(context));

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new CreateLoaiCommand(new CreateLoaiDto { Ten = "  Gia dụng  " }),
            CancellationToken.None));
    }

    [Fact]
    public async Task Update_loai_returns_not_found_for_missing_id()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var handler = new UpdateLoaiCommandHandler(new LoaiRepository(context));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(
            new UpdateLoaiCommand(999999, new UpdateLoaiDto { Ten = "Tên mới" }),
            CancellationToken.None));
    }

    [Fact]
    public async Task Delete_loai_rejects_category_that_has_products()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var handler = new DeleteLoaiCommandHandler(new LoaiRepository(context));

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new DeleteLoaiCommand(1),
            CancellationToken.None));
        Assert.NotNull(await context.Loais.FindAsync(1));
    }

    [Fact]
    public async Task Get_loai_list_projects_page_and_count()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var handler = new GetLoaiListQueryHandler(new LoaiRepository(context));

        var result = await handler.Handle(
            new GetLoaiListQuery(2, 1, "id", "asc"),
            CancellationToken.None);

        Assert.Equal(3, result.TotalItems);
        Assert.Equal(2, result.Page);
        Assert.Equal("Gia dụng", Assert.Single(result.Items).Ten);
    }

    [Fact]
    public async Task Create_sanpham_maps_category_name_and_trims_values()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var handler = new CreateSanPhamCommandHandler(new SanPhamRepository(context));

        var result = await handler.Handle(
            new CreateSanPhamCommand(new CreateSanPhamDto
            {
                TenSP = "  Bàn phím mới  ",
                MoTaSP = "  mô tả  ",
                Gia = 123000,
                SoLuong = 4,
                LoaiId = 1
            }),
            CancellationToken.None);

        Assert.True(result.Id > 0);
        Assert.Equal("Bàn phím mới", result.TenSP);
        Assert.Equal("mô tả", result.MoTaSP);
        Assert.Equal("Điện tử", result.TenLoai);
    }

    [Fact]
    public async Task Create_sanpham_rejects_nonexistent_category()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var handler = new CreateSanPhamCommandHandler(new SanPhamRepository(context));

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(
            new CreateSanPhamCommand(new CreateSanPhamDto
            {
                TenSP = "Không có loại",
                Gia = 10000,
                SoLuong = 1,
                LoaiId = 999999
            }),
            CancellationToken.None));
    }

    [Fact]
    public async Task Create_sanpham_rejects_duplicate_name_in_same_category()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var handler = new CreateSanPhamCommandHandler(new SanPhamRepository(context));

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new CreateSanPhamCommand(new CreateSanPhamDto
            {
                TenSP = "  Tai nghe Bluetooth ",
                Gia = 10000,
                SoLuong = 1,
                LoaiId = 1
            }),
            CancellationToken.None));
    }

    [Fact]
    public async Task Get_sanpham_list_searches_filters_and_projects_category()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var handler = new GetSanPhamListQueryHandler(new SanPhamRepository(context));

        var result = await handler.Handle(
            new GetSanPhamListQuery(1, 10, "Tai nghe", "gia", "desc", 1),
            CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal("Tai nghe Bluetooth", item.TenSP);
        Assert.Equal("Điện tử", item.TenLoai);
        Assert.Equal(1, result.TotalItems);
    }

    [Fact]
    public async Task Get_sanpham_by_id_returns_not_found_for_missing_id()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var handler = new GetSanPhamByIdQueryHandler(new SanPhamRepository(context));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(
            new GetSanPhamByIdQuery(999999),
            CancellationToken.None));
    }
}
