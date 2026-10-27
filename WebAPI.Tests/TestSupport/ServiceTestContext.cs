using Microsoft.EntityFrameworkCore;
using WebAPI.Data;

namespace WebAPI.Tests.TestSupport;

internal static class ServiceTestContext
{
    public static QLSanPhamDBContext CreateSeeded()
    {
        var options = new DbContextOptionsBuilder<QLSanPhamDBContext>()
            .UseInMemoryDatabase($"week4-tests-{Guid.NewGuid():N}")
            .Options;

        var context = new QLSanPhamDBContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
