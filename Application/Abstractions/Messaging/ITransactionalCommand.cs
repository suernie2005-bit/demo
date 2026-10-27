namespace QLSanPham.Application.Abstractions.Messaging;

/// <summary>
/// Marks a MediatR request that changes persisted state and must run in a transaction.
/// </summary>
public interface ITransactionalCommand
{
}
