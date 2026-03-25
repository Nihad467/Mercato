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
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");

            entity.Property(x => x.Price)
                .HasPrecision(18, 2);

            entity.HasOne(x => x.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("ProductImages");

            entity.Property(x => x.ObjectKey)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.Order)
                .HasDefaultValue(0);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public async Task AddProductAsync(Product product, CancellationToken cancellationToken)
    {
        await Products.AddAsync(product, cancellationToken);
    }

    public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await Products
            .Include(x => x.Category)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Mercato.Domain.Entities.Product>> GetAllProductsAsync(CancellationToken cancellationToken)
    {
        return await Products
            .Include(x => x.Category)
            .Include(x => x.Images)
            .ToListAsync(cancellationToken);
    }
    public void RemoveProduct(Product product)
    {
        Products.Remove(product);
    }

    public async Task AddCategoryAsync(Category category, CancellationToken cancellationToken)
    {
        await Categories.AddAsync(category, cancellationToken);
    }

    public async Task<Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken)
    {
        return await Categories.ToListAsync(cancellationToken);
    }

    public async Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken)
    {
        return await Categories.AnyAsync(x => x.Id == categoryId, cancellationToken);
    }

    public async Task<bool> CategoryHasProductsAsync(int categoryId, CancellationToken cancellationToken)
    {
        return await Products.AnyAsync(x => x.CategoryId == categoryId, cancellationToken);
    }

    public void RemoveCategory(Category category)
    {
        Categories.Remove(category);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
    public void RemoveProductImages(IEnumerable<Mercato.Domain.Entities.ProductImage> images)
    {
        ProductImages.RemoveRange(images);
    }
}