using MediatR;
using Microsoft.EntityFrameworkCore;
using QLSanPham.Application.Abstractions.Messaging;
using QLSanPham.Infrastructure.Persistence;

namespace WebAPI.Tests.Integration;

public sealed record SqlServerTransactionProbeCommand(bool ThrowAfterWrite)
    : IRequest<int>, ITransactionalCommand;

public sealed class SqlServerTransactionProbeHandler
    : IRequestHandler<SqlServerTransactionProbeCommand, int>
{
    private readonly QLSanPhamDBContext _dbContext;

    public SqlServerTransactionProbeHandler(QLSanPhamDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> Handle(
        SqlServerTransactionProbeCommand request,
        CancellationToken cancellationToken)
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
            "INSERT INTO #TransactionProbe (Id) VALUES (1);",
            cancellationToken);

        if (request.ThrowAfterWrite)
        {
            throw new InvalidOperationException("Simulated failure after the write.");
        }

        return 1;
    }
}
