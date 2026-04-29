using MediatR;
using Mercato.Application.Wishlist.Commands.AddToWishlist;
using Mercato.Application.Wishlist.Commands.RemoveFromWishlist;
using Mercato.Application.Wishlist.Queries.GetMyWishlist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mercato.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class WishlistController : ControllerBase
{
    private readonly IMediator _mediator;

    public WishlistController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyWishlist(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyWishlistQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{productId:int}")]
    public async Task<IActionResult> AddToWishlist(
        int productId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new AddToWishlistCommand(productId),
            cancellationToken);

        return Ok("Product added to wishlist successfully.");
    }

    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> RemoveFromWishlist(
        int productId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RemoveFromWishlistCommand(productId),
            cancellationToken);

        if (!result)
            return NotFound("Wishlist item not found.");

        return Ok("Product removed from wishlist successfully.");
    }
}