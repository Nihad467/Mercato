using Mercato.Domain.Entities;

namespace Mercato.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    Task AddProductAsync(Mercato.Domain.Entities.Product product, CancellationToken cancellationToken);

    Task<Mercato.Domain.Entities.Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}