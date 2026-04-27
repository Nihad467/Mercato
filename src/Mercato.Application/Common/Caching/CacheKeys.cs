namespace Mercato.Application.Common.Caching;

public static class CacheKeys
{
    public const string CategoriesList = "categories:list";

    public const string ProductsListPrefix = "products:list";

    public static string CategoryById(int categoryId)
    {
        return $"categories:detail:{categoryId}";
    }

    public static string ProductById(int productId)
    {
        return $"products:detail:{productId}";
    }

    public static string ProductList(
        int pageNumber,
        int pageSize,
        string? search,
        int? categoryId,
        string? sortBy)
    {
        return $"products:list:page={pageNumber}:size={pageSize}:search={search ?? "none"}:category={categoryId?.ToString() ?? "none"}:sort={sortBy ?? "none"}";
    }
}