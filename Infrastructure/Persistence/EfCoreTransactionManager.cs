using Microsoft.EntityFrameworkCore;
using QLSanPham.Application.Abstractions.Transactions;

namespace QLSanPham.Infrastructure.Persistence;

public sealed class EfCoreTransactionManager : ITransactionManager
{
    private readonly QLSanPhamDBContext _dbContext;

    public EfCoreTransactionManager(QLSanPhamDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TResponse> ExecuteAsync<TResponse>(
        Func<CancellationToken, Task<TResponse>> operation,
        CancellationToken cancellationToken)
    {
        // Reuse an outer transaction if a command invokes another command in the same scope.
        if (_dbContext.Database.CurrentTransaction is not null)
        {
            return await operation(cancellationToken);
        }

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
