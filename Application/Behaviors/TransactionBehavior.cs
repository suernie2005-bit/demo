using MediatR;
using QLSanPham.Application.Abstractions.Messaging;
using QLSanPham.Application.Abstractions.Transactions;

namespace QLSanPham.Application.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITransactionManager _transactionManager;

    public TransactionBehavior(ITransactionManager transactionManager)
    {
        _transactionManager = transactionManager;
    }

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ITransactionalCommand)
        {
            return next(cancellationToken);
        }

        return _transactionManager.ExecuteAsync(
            token => next(token),
            cancellationToken);
    }
}
