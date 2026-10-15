using WebAPI.DTOs.SanPham;
using WebAPI.Exceptions;

namespace WebAPI.Tests;

public class SanPhamServiceTests
{
    [Fact]
    public async Task CreateAsync_when_category_is_missing_throws_argument_exception()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreateSanPhamService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(new CreateSanPhamDto
        {
            TenSP = "Phone A",
            Gia = 100,
            SoLuong = 1,
            LoaiId = 999
        }));
    }

    [Fact]
    public async Task CreateAsync_when_same_name_exists_in_category_throws_conflict()
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
        var service = TestDbContextFactory.CreateSanPhamService(context);

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(new CreateSanPhamDto
        {
            TenSP = "Phone A",
            Gia = 200,
            SoLuong = 2,
            LoaiId = category.Id
        }));
    }

    [Fact]
    public async Task GetAllAsync_filters_sorts_and_pages()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var category = new WebAPI.Models.Loai { Ten = "Điện thoại" };
        context.Loais.Add(category);
        await context.SaveChangesAsync();
        context.SanPhams.AddRange(
            new WebAPI.Models.SanPham { TenSp = "Alpha", Gia = 100, SoLuong = 1, LoaiId = category.Id, Loai = category, Ngay = DateTime.UtcNow },
            new WebAPI.Models.SanPham { TenSp = "Beta", Gia = 200, SoLuong = 2, LoaiId = category.Id, Loai = category, Ngay = DateTime.UtcNow },
            new WebAPI.Models.SanPham { TenSp = "Gamma", Gia = 300, SoLuong = 3, LoaiId = category.Id, Loai = category, Ngay = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var service = TestDbContextFactory.CreateSanPhamService(context);

        var result = await service.GetAllAsync(2, 1, "a", "tensp", "desc", category.Id);

        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
        Assert.Single(result.Items);
        Assert.Equal("Beta", result.Items[0].TenSP);
        Assert.Equal("Điện thoại", result.Items[0].TenLoai);
    }

    [Fact]
    public async Task GetAllAsync_when_page_size_is_too_large_throws_argument_exception()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreateSanPhamService(context);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetAllAsync(1, 101, null, "id", "asc", null));
    }

    [Fact]
    public async Task UpdateAsync_when_product_is_missing_throws_key_not_found()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreateSanPhamService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateAsync(999, new UpdateSanPhamDto
        {
            TenSP = "Phone A",
            Gia = 100,
            SoLuong = 1,
            LoaiId = 1
        }));
    }

    [Fact]
    public async Task DeleteAsync_when_product_is_missing_includes_id_in_message()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var service = TestDbContextFactory.CreateSanPhamService(context);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(999));

        Assert.Contains("999", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_returns_created_product_with_category_name()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var category = new WebAPI.Models.Loai { Ten = "Điện thoại" };
        context.Loais.Add(category);
        await context.SaveChangesAsync();
        var service = TestDbContextFactory.CreateSanPhamService(context);

        var result = await service.CreateAsync(new CreateSanPhamDto
        {
            TenSP = "  Phone A  ",
            MoTaSP = "  Mô tả  ",
            Gia = 100,
            SoLuong = 1,
            LoaiId = category.Id
        });

        Assert.True(result.Id > 0);
        Assert.Equal("Phone A", result.TenSP);
        Assert.Equal("Mô tả", result.MoTaSP);
        Assert.Equal("Điện thoại", result.TenLoai);
    }

    [Fact]
    public async Task UpdateAsync_when_name_conflicts_throws_conflict()
    {
        await using var context = TestDbContextFactory.CreateContext();
        var category = new WebAPI.Models.Loai { Ten = "Điện thoại" };
        context.Loais.Add(category);
        await context.SaveChangesAsync();
        context.SanPhams.AddRange(
            new WebAPI.Models.SanPham { TenSp = "Phone A", Gia = 100, SoLuong = 1, LoaiId = category.Id, Loai = category, Ngay = DateTime.UtcNow },
            new WebAPI.Models.SanPham { TenSp = "Phone B", Gia = 200, SoLuong = 2, LoaiId = category.Id, Loai = category, Ngay = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var productToUpdate = context.SanPhams.Single(x => x.TenSp == "Phone B");
        var service = TestDbContextFactory.CreateSanPhamService(context);

        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateAsync(productToUpdate.Id, new UpdateSanPhamDto
        {
            TenSP = "Phone A",
            Gia = 250,
            SoLuong = 2,
            LoaiId = category.Id
        }));
    }
}
