using MediatR;
using Mercato.Application.Common.Interfaces;

namespace Mercato.Application.ProductReviews.Queries.GetProductReviews;

public class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, GetProductReviewsResult>
{
    private readonly IApplicationDbContext _context;

    public GetProductReviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetProductReviewsResult> Handle(
        GetProductReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _context.GetProductByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            throw new Exception("Product not found.");

        var reviews = await _context.GetProductReviewsAsync(
            request.ProductId,
            cancellationToken);

        var reviewCount = reviews.Count;

        var averageRating = reviewCount == 0
            ? 0
            : Math.Round((decimal)reviews.Average(x => x.Rating), 2);

        return new GetProductReviewsResult
        {
            ProductId = request.ProductId,
            AverageRating = averageRating,
            ReviewCount = reviewCount,
            Reviews = reviews.Select(x => new ProductReviewDto
            {
                ReviewId = x.Id,
                UserId = x.UserId,
                Rating = x.Rating,
                Comment = x.Comment,
                CreatedAtUtc = x.CreatedAtUtc
            }).ToList()
        };
    }
}