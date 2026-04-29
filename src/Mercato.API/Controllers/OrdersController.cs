using MediatR;
using Mercato.Application.Orders.Commands.CreateOrder;
using Mercato.Application.Orders.Queries.GetMyOrderById;
using Mercato.Application.Orders.Queries.GetMyOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mercato.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateOrder(
     [FromBody] CreateOrderCommand command,
     CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var result = await _mediator.Send(new GetMyOrdersQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMyOrderById(int id)
    {
        var result = await _mediator.Send(new GetMyOrderByIdQuery(id));
        return Ok(result);
    }
}