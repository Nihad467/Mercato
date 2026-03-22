namespace Mercato.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    Task AddProductAsync(Mercato.Domain.Entities.Product product, CancellationToken cancellationToken);

    Task<Mercato.Domain.Entities.Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken);

    Task<List<Mercato.Domain.Entities.Product>> GetAllProductsAsync(CancellationToken cancellationToken);

    void RemoveProduct(Mercato.Domain.Entities.Product product);

    Task AddCategoryAsync(Mercato.Domain.Entities.Category category, CancellationToken cancellationToken);

    Task<Mercato.Domain.Entities.Category?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken);

    Task<List<Mercato.Domain.Entities.Category>> GetAllCategoriesAsync(CancellationToken cancellationToken);

    void RemoveCategory(Mercato.Domain.Entities.Category category);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}