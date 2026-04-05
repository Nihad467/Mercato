using Mercato.Domain.Entities;

namespace Mercato.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    Task AddProductAsync(Mercato.Domain.Entities.Product product, CancellationToken cancellationToken);
    Task<Mercato.Domain.Entities.Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<Mercato.Domain.Entities.Product>> GetAllProductsAsync(CancellationToken cancellationToken);
    void RemoveProduct(Mercato.Domain.Entities.Product product);
    void RemoveProductImages(IEnumerable<Mercato.Domain.Entities.ProductImage> images);

    Task AddCategoryAsync(Mercato.Domain.Entities.Category category, CancellationToken cancellationToken);
    Task<Mercato.Domain.Entities.Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<Mercato.Domain.Entities.Category>> GetAllCategoriesAsync(CancellationToken cancellationToken);
    Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken);
    Task<bool> CategoryHasProductsAsync(int categoryId, CancellationToken cancellationToken);

    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken);
    void RemoveRefreshToken(RefreshToken refreshToken);

    void RemoveCategory(Mercato.Domain.Entities.Category category);

    Task<List<CartItem>> GetUserCartItemsAsync(Guid userId, CancellationToken cancellationToken);
    Task<CartItem?> GetCartItemAsync(Guid userId, int productId, CancellationToken cancellationToken);
    Task<CartItem?> GetCartItemByIdAsync(int cartItemId, Guid userId, CancellationToken cancellationToken);
    Task AddCartItemAsync(CartItem cartItem, CancellationToken cancellationToken);
    void RemoveCartItem(CartItem cartItem);

    Task AddOrderAsync(Order order, CancellationToken cancellationToken);
    Task<Order?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken);
    Task<Order?> GetUserOrderByIdAsync(int orderId, Guid userId, CancellationToken cancellationToken);
    Task<List<Order>> GetUserOrdersAsync(Guid userId, CancellationToken cancellationToken);
    Task<List<Order>> GetAllOrdersAsync(CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}