namespace QLSanPham.Application.Abstractions.Transactions;

public interface ITransactionManager
{
    Task<TResponse> ExecuteAsync<TResponse>(
        Func<CancellationToken, Task<TResponse>> operation,
        CancellationToken cancellationToken);
}
