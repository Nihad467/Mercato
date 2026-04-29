using MediatR;

namespace Mercato.Application.ProductReviews.Commands.DeleteProductReview;

public class DeleteProductReviewCommand : IRequest<bool>
{
    public int ReviewId { get; set; }

    public DeleteProductReviewCommand(int reviewId)
    {
        ReviewId = reviewId;
    }
}