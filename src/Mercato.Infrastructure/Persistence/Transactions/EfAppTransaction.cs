using Mercato.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mercato.Infrastructure.Persistence.Transactions;

public class EfAppTransaction : IAppTransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfAppTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        await _transaction.RollbackAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
    }
}