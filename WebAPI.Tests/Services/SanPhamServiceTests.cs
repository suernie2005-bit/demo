using Microsoft.Extensions.Logging.Abstractions;
using WebAPI.DTOs.SanPham;
using WebAPI.Services;
using WebAPI.Tests.TestSupport;

namespace WebAPI.Tests.Services;

public class SanPhamServiceTests
{
    [Fact]
    public async Task GetAllAsync_RejectsPageSizeBelowOne()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAllAsync(1, 0, null, "id", "asc", null));
    }

    [Fact]
    public async Task GetAllAsync_RejectsUnsupportedSortField()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAllAsync(1, 10, null, "unknown", "asc", null));
    }

    [Fact]
    public async Task GetAllAsync_RejectsUnsupportedSortOrder()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAllAsync(1, 10, null, "id", "random", null));
    }

    [Fact]
    public async Task GetAllAsync_RejectsNonPositiveCategoryFilter()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAllAsync(1, 10, null, "id", "asc", 0));
    }

    [Fact]
    public async Task GetAllAsync_SearchesFiltersAndProjectsProduct()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        var result = await service.GetAllAsync(
            page: 1,
            pageSize: 10,
            search: "Tai nghe",
            sortBy: "gia",
            sortOrder: "desc",
            loaiId: 1);

        var product = Assert.Single(result.Items);
        Assert.Equal("Tai nghe Bluetooth", product.TenSP);
        Assert.Equal(1, product.LoaiId);
        Assert.Equal("Điện tử", product.TenLoai);
        Assert.Equal(1, result.TotalItems);
    }

    [Fact]
    public async Task GetAllAsync_EmptySearchAndUnknownPositiveCategoryReturnEmptyPage()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        var result = await service.GetAllAsync(1, 10, "   ", "id", "asc", 999999);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyForPageBeyondIntRange()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        var result = await service.GetAllAsync(int.MaxValue, 100, null, "id", "asc", null);

        Assert.Empty(result.Items);
        Assert.Equal(3, result.TotalItems);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsWhenProductDoesNotExist()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetByIdAsync(999999));
    }

    [Fact]
    public async Task CreateAsync_RejectsUnknownCategory()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CreateSanPhamDto
        {
            TenSP = "Sản phẩm mới",
            Gia = 10000,
            SoLuong = 1,
            LoaiId = 999999
        }));
    }

    [Fact]
    public async Task CreateAsync_RejectsWhitespaceOnlyName()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CreateSanPhamDto
        {
            TenSP = "   ",
            Gia = 10000,
            SoLuong = 1,
            LoaiId = 1
        }));
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateNameWithinSameCategoryAfterTrimming()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new CreateSanPhamDto
        {
            TenSP = "  Tai nghe Bluetooth  ",
            Gia = 10000,
            SoLuong = 1,
            LoaiId = 1
        }));
    }

    [Fact]
    public async Task CreateAsync_TrimsNameAndDescription()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        var result = await service.CreateAsync(new CreateSanPhamDto
        {
            TenSP = "  Bàn phím cơ  ",
            MoTaSP = "  Bàn phím mới  ",
            Gia = 750000,
            SoLuong = 5,
            LoaiId = 1
        });

        Assert.Equal("Bàn phím cơ", result.TenSP);
        Assert.Equal("Bàn phím mới", result.MoTaSP);
        Assert.Equal("Điện tử", result.TenLoai);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsWhenProductDoesNotExist()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateAsync(
            999999,
            new UpdateSanPhamDto
            {
                TenSP = "Không tồn tại",
                Gia = 10000,
                SoLuong = 1,
                LoaiId = 1
            }));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenProductDoesNotExist()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(999999));
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_SupportsProductCrud()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        var created = await service.CreateAsync(new CreateSanPhamDto
        {
            TenSP = "Máy tính bảng",
            Gia = 4000000,
            SoLuong = 2,
            LoaiId = 1
        });

        var updated = await service.UpdateAsync(created.Id, new UpdateSanPhamDto
        {
            TenSP = "Máy tính bảng 2",
            Gia = 4500000,
            SoLuong = 1,
            LoaiId = 1
        });

        Assert.Equal("Máy tính bảng 2", updated.TenSP);
        Assert.Equal(4500000, updated.Gia);

        await service.DeleteAsync(created.Id);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync(created.Id));
    }

    private static SanPhamService CreateService(WebAPI.Data.QLSanPhamDBContext context)
    {
        return new SanPhamService(context, NullLogger<SanPhamService>.Instance);
    }
}
