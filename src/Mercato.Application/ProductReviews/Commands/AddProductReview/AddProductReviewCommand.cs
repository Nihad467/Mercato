using MediatR;

namespace Mercato.Application.ProductReviews.Commands.AddProductReview;

public class AddProductReviewCommand : IRequest<bool>
{
    public int ProductId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }
}