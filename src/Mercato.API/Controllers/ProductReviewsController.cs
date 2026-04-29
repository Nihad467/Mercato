using MediatR;
using Mercato.Application.ProductReviews.Commands.AddProductReview;
using Mercato.Application.ProductReviews.Commands.DeleteProductReview;
using Mercato.Application.ProductReviews.Queries.GetProductReviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mercato.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProductReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> GetProductReviews(
        int productId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetProductReviewsQuery(productId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddReview(
        [FromBody] AddProductReviewCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok("Review added successfully.");
    }

    [HttpDelete("{reviewId:int}")]
    public async Task<IActionResult> DeleteReview(
        int reviewId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteProductReviewCommand(reviewId),
            cancellationToken);

        if (!result)
            return NotFound("Review not found.");

        return Ok("Review deleted successfully.");
    }
}