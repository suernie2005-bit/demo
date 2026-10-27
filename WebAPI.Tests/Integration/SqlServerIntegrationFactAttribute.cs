using Xunit;

namespace WebAPI.Tests.Integration;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class SqlServerIntegrationFactAttribute : FactAttribute
{
    public SqlServerIntegrationFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable(SqlServerTransactionTestScope.ConnectionEnvironmentVariable)))
        {
            Skip = $"Set {SqlServerTransactionTestScope.ConnectionEnvironmentVariable} to run SQL Server integration tests.";
        }
    }
}
