using MediatR;

namespace Mercato.Application.ProductReviews.Queries.GetProductReviews;

public class GetProductReviewsQuery : IRequest<GetProductReviewsResult>
{
    public int ProductId { get; set; }

    public GetProductReviewsQuery(int productId)
    {
        ProductId = productId;
    }
}