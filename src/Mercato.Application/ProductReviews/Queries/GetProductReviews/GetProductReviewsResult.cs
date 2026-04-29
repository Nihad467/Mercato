namespace Mercato.Application.ProductReviews.Queries.GetProductReviews;

public class GetProductReviewsResult
{
    public int ProductId { get; set; }

    public decimal AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public List<ProductReviewDto> Reviews { get; set; } = new();
}