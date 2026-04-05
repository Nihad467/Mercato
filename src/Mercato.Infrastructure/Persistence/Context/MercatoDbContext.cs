using Mercato.Application.Common.Interfaces;
using Mercato.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mercato.Infrastructure.Persistence.Context;

public class MercatoDbContext
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public MercatoDbContext(DbContextOptions<MercatoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

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

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasIndex(x => x.Token)
                .IsUnique();

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.Property(x => x.ExpiresAtUtc)
                .IsRequired();

            entity.Property(x => x.IsRevoked)
                .IsRequired();

            entity.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("CartItems");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.Property(x => x.Quantity)
                .IsRequired();

            entity.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.UserId, x.ProductId })
                .IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .IsRequired();

            entity.Property(x => x.TotalPrice)
                .HasPrecision(18, 2);

            entity.Property(x => x.Status)
                .IsRequired();

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.HasMany(x => x.OrderItems)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.UnitPrice)
                .HasPrecision(18, 2);

            entity.Property(x => x.TotalPrice)
                .HasPrecision(18, 2);

            entity.Property(x => x.Quantity)
                .IsRequired();
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

    public async Task<List<Product>> GetAllProductsAsync(CancellationToken cancellationToken)
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

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await RefreshTokens.FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
    }

    public void RemoveRefreshToken(RefreshToken refreshToken)
    {
        RefreshTokens.Remove(refreshToken);
    }

    public void RemoveProductImages(IEnumerable<ProductImage> images)
    {
        ProductImages.RemoveRange(images);
    }

    public async Task<List<CartItem>> GetUserCartItemsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await CartItems
            .Include(x => x.Product)
            .ThenInclude(x => x.Images)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<CartItem?> GetCartItemAsync(Guid userId, int productId, CancellationToken cancellationToken)
    {
        return await CartItems
            .Include(x => x.Product)
            .ThenInclude(x => x.Images)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId, cancellationToken);
    }

    public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId, Guid userId, CancellationToken cancellationToken)
    {
        return await CartItems
            .Include(x => x.Product)
            .ThenInclude(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == cartItemId && x.UserId == userId, cancellationToken);
    }

    public async Task AddCartItemAsync(CartItem cartItem, CancellationToken cancellationToken)
    {
        await CartItems.AddAsync(cartItem, cancellationToken);
    }

    public void RemoveCartItem(CartItem cartItem)
    {
        CartItems.Remove(cartItem);
    }

    public async Task AddOrderAsync(Order order, CancellationToken cancellationToken)
    {
        await Orders.AddAsync(order, cancellationToken);
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken)
    {
        return await Orders
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
    }

    public async Task<Order?> GetUserOrderByIdAsync(int orderId, Guid userId, CancellationToken cancellationToken)
    {
        return await Orders
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.Id == orderId && x.UserId == userId, cancellationToken);
    }

    public async Task<List<Order>> GetUserOrdersAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await Orders
            .Include(x => x.OrderItems)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Order>> GetAllOrdersAsync(CancellationToken cancellationToken)
    {
        return await Orders
            .Include(x => x.OrderItems)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}