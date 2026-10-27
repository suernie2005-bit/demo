using Xunit;

namespace WebAPI.Tests.Integration;

public sealed class SqlServerTransactionBehaviorTests
{
    [SqlServerIntegrationFact]
    public async Task Transactional_command_commits_when_handler_succeeds()
    {
        await using var testScope = await SqlServerTransactionTestScope.CreateAsync();

        await testScope.Sender.Send(new SqlServerTransactionProbeCommand(ThrowAfterWrite: false));

        Assert.Equal(1, await testScope.CountRowsAsync());
    }

    [SqlServerIntegrationFact]
    public async Task Transactional_command_rolls_back_when_handler_fails_after_write()
    {
        await using var testScope = await SqlServerTransactionTestScope.CreateAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => testScope.Sender.Send(new SqlServerTransactionProbeCommand(ThrowAfterWrite: true)));

        Assert.Equal(0, await testScope.CountRowsAsync());
    }
}
