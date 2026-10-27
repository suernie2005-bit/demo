using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace QLSanPham.Infrastructure.Persistence;

public sealed class QLSanPhamDBContextFactory : IDesignTimeDbContextFactory<QLSanPhamDBContext>
{
    public QLSanPhamDBContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<QLSanPhamDBContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var optionsBuilder = new DbContextOptionsBuilder<QLSanPhamDBContext>();

        // A local placeholder lets `migrations add` build the model before credentials
        // are configured. `database update` must use a real User Secrets connection string.
        optionsBuilder.UseSqlServer(
            string.IsNullOrWhiteSpace(connectionString)
                ? "Server=localhost;Database=QLSanphamDB;Trusted_Connection=True;TrustServerCertificate=True;"
                : connectionString);

        return new QLSanPhamDBContext(optionsBuilder.Options);
    }
}
