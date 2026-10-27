using System.Data.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QLSanPham.Application;
using QLSanPham.Application.Abstractions.Transactions;
using QLSanPham.Infrastructure.Persistence;

namespace WebAPI.Tests.Integration;

internal sealed class SqlServerTransactionTestScope : IAsyncDisposable
{
    public const string ConnectionEnvironmentVariable = "QLSANPHAM_SQLSERVER_TEST_CONNECTION";
    private const string RequiredDatabaseName = "QLSanphamDB_Week5_Test";

    private readonly ServiceProvider _rootProvider;
    private readonly AsyncServiceScope _serviceScope;

    private SqlServerTransactionTestScope(
        ServiceProvider rootProvider,
        AsyncServiceScope serviceScope,
        QLSanPhamDBContext dbContext)
    {
        _rootProvider = rootProvider;
        _serviceScope = serviceScope;
        DbContext = dbContext;
    }

    public QLSanPhamDBContext DbContext { get; }

    public ISender Sender => _serviceScope.ServiceProvider.GetRequiredService<ISender>();

    public static async Task<SqlServerTransactionTestScope> CreateAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Set {ConnectionEnvironmentVariable} to a dedicated SQL Server test database.");
        }

        EnsureDedicatedTestDatabase(connectionString);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<QLSanPhamDBContext>(options => options.UseSqlServer(connectionString));
        services.AddApplication();
        services.AddScoped<ITransactionManager, EfCoreTransactionManager>();
        services.AddTransient<
            IRequestHandler<SqlServerTransactionProbeCommand, int>,
            SqlServerTransactionProbeHandler>();

        var rootProvider = services.BuildServiceProvider(validateScopes: true);
        var serviceScope = rootProvider.CreateAsyncScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<QLSanPhamDBContext>();

        try
        {
            await dbContext.Database.OpenConnectionAsync();
            await dbContext.Database.ExecuteSqlRawAsync(
                "CREATE TABLE #TransactionProbe (Id INT NOT NULL PRIMARY KEY);");

            return new SqlServerTransactionTestScope(rootProvider, serviceScope, dbContext);
        }
        catch
        {
            await serviceScope.DisposeAsync();
            await rootProvider.DisposeAsync();
            throw;
        }
    }

    public async Task<int> CountRowsAsync()
    {
        await using var command = DbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM #TransactionProbe;";
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await DbContext.Database.ExecuteSqlRawAsync(
                "DROP TABLE IF EXISTS #TransactionProbe;");
        }
        finally
        {
            await DbContext.Database.CloseConnectionAsync();
            await _serviceScope.DisposeAsync();
            await _rootProvider.DisposeAsync();
        }
    }

    private static void EnsureDedicatedTestDatabase(string connectionString)
    {
        var parsed = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        object? databaseName = null;
        if (!parsed.TryGetValue("Initial Catalog", out databaseName)
            && !parsed.TryGetValue("Database", out databaseName))
        {
            throw new InvalidOperationException(
                $"The SQL integration-test connection string must explicitly target {RequiredDatabaseName}.");
        }

        if (!string.Equals(
                Convert.ToString(databaseName),
                RequiredDatabaseName,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"SQL integration tests are restricted to database {RequiredDatabaseName}.");
        }
    }
}
