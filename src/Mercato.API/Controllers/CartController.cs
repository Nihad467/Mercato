using MediatR;
using Mercato.Application.Carts.Commands.AddToCart;
using Mercato.Application.Carts.Commands.RemoveFromCart;
using Mercato.Application.Carts.Commands.UpdateCartItemQuantity;
using Mercato.Application.Carts.Queries.GetMyCart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mercato.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyCart()
    {
        var result = await _mediator.Send(new GetMyCartQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(AddToCartCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateQuantity(UpdateCartItemQuantityCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{cartItemId}")]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        await _mediator.Send(new RemoveFromCartCommand(cartItemId));
        return NoContent();
    }
}