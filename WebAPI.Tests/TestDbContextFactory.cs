using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WebAPI.Data;
using WebAPI.Services;

namespace WebAPI.Tests;

internal static class TestDbContextFactory
{
    public static QLSanPhamDBContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<QLSanPhamDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new QLSanPhamDBContext(options);
    }

    public static LoaiService CreateLoaiService(QLSanPhamDBContext context) =>
        new(context, NullLogger<LoaiService>.Instance);

    public static SanPhamService CreateSanPhamService(QLSanPhamDBContext context) =>
        new(context, NullLogger<SanPhamService>.Instance);
}
