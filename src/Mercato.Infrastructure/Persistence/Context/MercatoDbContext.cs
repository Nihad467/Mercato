using Microsoft.EntityFrameworkCore;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Infrastructure.Persistence.Context;

public class MercatoDbContext : DbContext, IApplicationDbContext
{
    public MercatoDbContext(DbContextOptions<MercatoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Mercato.Domain.Entities.Product> Products => Set<Mercato.Domain.Entities.Product>();
    public DbSet<Mercato.Domain.Entities.Category> Categories => Set<Mercato.Domain.Entities.Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Mercato.Domain.Entities.Product>().ToTable("Products");
        modelBuilder.Entity<Mercato.Domain.Entities.Category>().ToTable("Category");
    }

    public async Task AddProductAsync(Mercato.Domain.Entities.Product product, CancellationToken cancellationToken)
    {
        await Products.AddAsync(product, cancellationToken);
    }

    public async Task<Mercato.Domain.Entities.Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await Products
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Mercato.Domain.Entities.Product>> GetAllProductsAsync(CancellationToken cancellationToken)
    {
        return await Products.ToListAsync(cancellationToken);
    }

    public void RemoveProduct(Mercato.Domain.Entities.Product product)
    {
        Products.Remove(product);
    }

    public async Task AddCategoryAsync(Mercato.Domain.Entities.Category category, CancellationToken cancellationToken)
    {
        await Categories.AddAsync(category, cancellationToken);
    }

    public async Task<Mercato.Domain.Entities.Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Mercato.Domain.Entities.Category>> GetAllCategoriesAsync(CancellationToken cancellationToken)
    {
        return await Categories.ToListAsync(cancellationToken);
    }

    public void RemoveCategory(Mercato.Domain.Entities.Category category)
    {
        Categories.Remove(category);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}