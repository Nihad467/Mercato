namespace Mercato.Application.ProductReviews.Queries.GetProductReviews;

public class ProductReviewDto
{
    public int ReviewId { get; set; }

    public Guid UserId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}