using MediatR;
using QLSanPham.Application.Abstractions.Messaging;
using QLSanPham.Application.Abstractions.Transactions;
using QLSanPham.Application.Behaviors;

namespace WebAPI.Tests;

public sealed class TransactionBehaviorTests
{
    [Fact]
    public async Task Transactional_command_runs_inside_transaction_manager()
    {
        var transactionManager = new RecordingTransactionManager();
        var behavior = new TransactionBehavior<TestCommand, int>(transactionManager);
        var handlerCalled = false;

        int result = await behavior.Handle(
            new TestCommand(),
            _ =>
            {
                handlerCalled = true;
                return Task.FromResult(42);
            },
            CancellationToken.None);

        Assert.Equal(42, result);
        Assert.True(handlerCalled);
        Assert.Equal(1, transactionManager.ExecutionCount);
    }

    [Fact]
    public async Task Query_does_not_open_write_transaction()
    {
        var transactionManager = new RecordingTransactionManager();
        var behavior = new TransactionBehavior<TestQuery, int>(transactionManager);

        int result = await behavior.Handle(
            new TestQuery(),
            _ => Task.FromResult(7),
            CancellationToken.None);

        Assert.Equal(7, result);
        Assert.Equal(0, transactionManager.ExecutionCount);
    }

    private sealed record TestCommand : IRequest<int>, ITransactionalCommand;

    private sealed record TestQuery : IRequest<int>;

    private sealed class RecordingTransactionManager : ITransactionManager
    {
        public int ExecutionCount { get; private set; }

        public Task<TResponse> ExecuteAsync<TResponse>(
            Func<CancellationToken, Task<TResponse>> operation,
            CancellationToken cancellationToken)
        {
            ExecutionCount++;
            return operation(cancellationToken);
        }
    }
}
