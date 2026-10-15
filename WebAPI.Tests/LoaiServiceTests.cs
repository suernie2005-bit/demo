using WebAPI.DTOs.Loai;
using WebAPI.Exceptions;

namespace WebAPI.Tests;

public class LoaiServiceTests
{
    [Fact]
    public async Task GetByIdAsync_when_missing_throws_key_not_found()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreateLoaiService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync(999));
    }

    [Fact]
    public async Task CreateAsync_when_name_exists_throws_conflict()
    {
        await using var context = TestDbContextFactory.CreateContext();
        context.Loais.Add(new WebAPI.Models.Loai { Ten = "Điện thoại" });
        await context.SaveChangesAsync();
        var service = TestDbContextFactory.CreateLoaiService(context);

        var exception = await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(
            new CreateLoaiDto { Ten = "Điện thoại" }));

        Assert.Equal("Tên loại đã tồn tại.", exception.Message);
    }

    [Fact]
    public async Task DeleteAsync_when_category_has_products_throws_conflict()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var category = new WebAPI.Models.Loai { Ten = "Điện thoại" };
        context.Loais.Add(category);
        await context.SaveChangesAsync();
        context.SanPhams.Add(new WebAPI.Models.SanPham
        {
            TenSp = "Phone A",
            Gia = 100,
            SoLuong = 1,
            LoaiId = category.Id,
            Loai = category,
            Ngay = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var service = TestDbContextFactory.CreateLoaiService(context);

        await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(category.Id));
    }

    [Fact]
    public async Task GetAllAsync_when_page_is_invalid_throws_argument_exception()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreateLoaiService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAllAsync(0, 10, "id", "asc"));
    }

    [Fact]
    public async Task CreateAsync_trims_name_and_description()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreateLoaiService(context);

        var result = await service.CreateAsync(new CreateLoaiDto
        {
            Ten = "  Điện thoại  ",
            Mota = "  Thiết bị  "
        });

        Assert.Equal("Điện thoại", result.Ten);
        Assert.Equal("Thiết bị", result.Mota);
    }
}
