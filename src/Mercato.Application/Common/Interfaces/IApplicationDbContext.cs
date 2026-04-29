using Mercato.Application.Admin.Dashboard.Dtos;
using Mercato.Application.Ai.Models;
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

    Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken);
    Task<Payment?> GetPaymentByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Payment?> GetPaymentByOrderIdAsync(int orderId, CancellationToken cancellationToken);
    Task<List<Payment>> GetPaymentsByOrderIdAsync(int orderId, CancellationToken cancellationToken);
    Task<Payment?> GetPaymentByExternalPaymentIdAsync(string externalPaymentId, CancellationToken cancellationToken);

    void UpdatePayment(Payment payment);
    Task<IAppTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    void UpdateOrder(Order order);
    Task<int> GetTotalOrdersCountAsync(CancellationToken cancellationToken = default);

    Task<decimal> GetTotalRevenueAsync(CancellationToken cancellationToken = default);

    Task<int> GetTotalProductsCountAsync(CancellationToken cancellationToken = default);

    Task<int> GetTotalUsersCountAsync(CancellationToken cancellationToken = default);

    Task<int> GetLowStockProductsCountAsync(
        int threshold,
        CancellationToken cancellationToken = default);

    Task<List<TopProductDto>> GetTopProductsAsync(
    int take,
    CancellationToken cancellationToken = default);
    Task<int> GetPendingOrdersCountAsync(CancellationToken cancellationToken = default);
    Task<List<LowStockProductDto>> GetLowStockProductsAsync(
    int threshold,
    CancellationToken cancellationToken = default);
    Task<List<RecentOrderDto>> GetRecentOrdersAsync(
    int take,
    CancellationToken cancellationToken = default);
    Task<RevenueSummaryDto> GetRevenueSummaryAsync(
    CancellationToken cancellationToken = default);

    Task<List<CategoryStatsDto>> GetCategoryStatsAsync(
        CancellationToken cancellationToken = default);
    Task<List<AiProductCandidateDto>> GetAiProductCandidatesAsync(
    CancellationToken cancellationToken = default);
    Task AddCouponAsync(Coupon coupon, CancellationToken cancellationToken);
    Task<Coupon?> GetCouponByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Coupon?> GetCouponByCodeAsync(string code, CancellationToken cancellationToken);
    Task<List<Coupon>> GetAllCouponsAsync(CancellationToken cancellationToken);
    void RemoveCoupon(Coupon coupon);
    Task<bool> CouponCodeExistsAsync(string code, CancellationToken cancellationToken);
    Task AddWishlistItemAsync(WishlistItem wishlistItem, CancellationToken cancellationToken);

    Task<WishlistItem?> GetWishlistItemAsync(
        Guid userId,
        int productId,
        CancellationToken cancellationToken);

    Task<List<WishlistItem>> GetUserWishlistItemsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    void RemoveWishlistItem(WishlistItem wishlistItem);
    Task AddProductReviewAsync(ProductReview review, CancellationToken cancellationToken);

    Task<ProductReview?> GetUserProductReviewAsync(
        Guid userId,
        int productId,
        CancellationToken cancellationToken);

    Task<ProductReview?> GetProductReviewByIdAsync(
        int reviewId,
        CancellationToken cancellationToken);

    Task<List<ProductReview>> GetProductReviewsAsync(
        int productId,
        CancellationToken cancellationToken);

    Task<bool> UserHasPurchasedProductAsync(
        Guid userId,
        int productId,
        CancellationToken cancellationToken);

    void RemoveProductReview(ProductReview review);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}