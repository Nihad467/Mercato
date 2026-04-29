using MediatR;
using Mercato.Application.Common.Interfaces;
using Mercato.Domain.Entities;

namespace Mercato.Application.ProductReviews.Commands.AddProductReview;

public class AddProductReviewCommandHandler : IRequestHandler<AddProductReviewCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AddProductReviewCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(AddProductReviewCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        if (request.Rating < 1 || request.Rating > 5)
            throw new Exception("Rating must be between 1 and 5.");

        var product = await _context.GetProductByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            throw new Exception("Product not found.");

        var hasPurchased = await _context.UserHasPurchasedProductAsync(
            userId,
            request.ProductId,
            cancellationToken);

        if (!hasPurchased)
            throw new Exception("You can review only products you have purchased.");

        var existingReview = await _context.GetUserProductReviewAsync(
            userId,
            request.ProductId,
            cancellationToken);

        if (existingReview is not null)
            throw new Exception("You have already reviewed this product.");

        var review = new ProductReview
        {
            UserId = userId,
            ProductId = request.ProductId,
            Rating = request.Rating,
            Comment = request.Comment?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        await _context.AddProductReviewAsync(review, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}