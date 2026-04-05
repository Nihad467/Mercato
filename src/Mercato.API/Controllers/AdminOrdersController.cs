using MediatR;
using Mercato.Application.Orders.Commands.UpdateOrderStatus;
using Mercato.Application.Orders.Queries.GetAllOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mercato.API.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        var result = await _mediator.Send(new GetAllOrdersQuery());
        return Ok(result);
    }

    [HttpPut("status")]
    public async Task<IActionResult> UpdateStatus(UpdateOrderStatusCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}