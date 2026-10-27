using Microsoft.Data.SqlClient;

namespace WebAPI.Tests.Integration;

public sealed class SqlServerIdentitySchemaTests
{
    [SqlServerIntegrationFact]
    public async Task Dedicated_database_contains_catalog_and_identity_migrations()
    {
        string connectionString = Environment.GetEnvironmentVariable(
            SqlServerTransactionTestScope.ConnectionEnvironmentVariable)!;
        var connectionOptions = new SqlConnectionStringBuilder(connectionString);

        Assert.Equal(
            "QLSanphamDB_Week5_Test",
            connectionOptions.InitialCatalog,
            ignoreCase: true);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM [__EFMigrationsHistory]
            WHERE [MigrationId] IN (
                N'20261027072150_InitialCreate',
                N'20261027121839_AddIdentityAndRefreshTokenPersistence');
            """;

        int appliedMigrationCount = Convert.ToInt32(await command.ExecuteScalarAsync());

        Assert.Equal(2, appliedMigrationCount);
    }
}
