using Microsoft.Extensions.Logging.Abstractions;
using QLSanPham.Application.DTOs.Loai;
using QLSanPham.Infrastructure.Persistence;
using QLSanPham.Infrastructure.Services;
using WebAPI.Tests.TestSupport;

namespace WebAPI.Tests.Services;

public class LoaiServiceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetAllAsync_RejectsPageBelowOne(int page)
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAllAsync(page, 10, "id", "asc"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task GetAllAsync_RejectsPageSizeOutsideAllowedRange(int pageSize)
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAllAsync(1, pageSize, "id", "asc"));
    }

    [Fact]
    public async Task GetAllAsync_RejectsUnsupportedSortField()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAllAsync(1, 10, "createdAt", "asc"));
    }

    [Fact]
    public async Task GetAllAsync_RejectsUnsupportedSortOrder()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetAllAsync(1, 10, "id", "sideways"));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsRequestedPageAndTotalCount()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        var result = await service.GetAllAsync(2, 2, "id", "asc");

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(2, result.TotalPages);
        Assert.Single(result.Items);
        Assert.Equal(3, result.Items[0].Id);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyItemsForPageBeyondData()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        var result = await service.GetAllAsync(100, 10, "id", "asc");

        Assert.Empty(result.Items);
        Assert.Equal(3, result.TotalItems);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyItemsForOffsetBeyondIntRange()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        var result = await service.GetAllAsync(int.MaxValue, 100, "id", "asc");

        Assert.Empty(result.Items);
        Assert.Equal(3, result.TotalItems);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsWhenCategoryDoesNotExist()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GetByIdAsync(999999));
    }

    [Fact]
    public async Task CreateAsync_RejectsWhitespaceOnlyName()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(new CreateLoaiDto { Ten = "   " }));
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateNameAfterTrimming()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateLoaiDto { Ten = "  Gia dụng  " }));
    }

    [Fact]
    public async Task CreateAsync_TrimsNameAndDescription()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        var result = await service.CreateAsync(new CreateLoaiDto
        {
            Ten = "  Đồ công nghệ  ",
            Mota = "  Thiết bị mới  "
        });

        Assert.Equal("Đồ công nghệ", result.Ten);
        Assert.Equal("Thiết bị mới", result.Mota);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsWhenCategoryDoesNotExist()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateAsync(999999, new UpdateLoaiDto { Ten = "Loại mới" }));
    }

    [Fact]
    public async Task DeleteAsync_RejectsCategoryThatHasProducts()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DeleteAsync(1));
    }

    [Fact]
    public async Task CreateUpdateAndDeleteAsync_SupportsCategoryCrud()
    {
        using var context = ServiceTestContext.CreateSeeded();
        var service = CreateService(context);

        var created = await service.CreateAsync(new CreateLoaiDto { Ten = "Loại tạm" });
        var updated = await service.UpdateAsync(created.Id, new UpdateLoaiDto { Ten = "Loại đã sửa" });

        Assert.Equal("Loại đã sửa", updated.Ten);

        await service.DeleteAsync(created.Id);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync(created.Id));
    }

    private static LoaiService CreateService(QLSanPhamDBContext context)
    {
        return new LoaiService(context, NullLogger<LoaiService>.Instance);
    }
}
