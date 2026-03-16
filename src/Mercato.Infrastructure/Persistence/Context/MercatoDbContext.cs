using Microsoft.EntityFrameworkCore;
using Mercato.Application.Common.Interfaces;
using Mercato.Domain.Entities;

namespace Mercato.Infrastructure.Persistence.Context;

public class MercatoDbContext : DbContext, IApplicationDbContext
{
    public MercatoDbContext(DbContextOptions<MercatoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public async Task AddProductAsync(Product product, CancellationToken cancellationToken)
    {
        await Products.AddAsync(product, cancellationToken);
    }

    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}