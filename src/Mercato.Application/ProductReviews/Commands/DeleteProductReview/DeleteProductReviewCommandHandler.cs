using MediatR;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.ProductReviews.Commands.DeleteProductReview;

public class DeleteProductReviewCommandHandler : IRequestHandler<DeleteProductReviewCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteProductReviewCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteProductReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var review = await _context.GetProductReviewByIdAsync(
            request.ReviewId,
            cancellationToken);

        if (review is null)
            return false;

        if (review.UserId != userId)
            throw new UnauthorizedAccessException("You can delete only your own review.");

        _context.RemoveProductReview(review);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}