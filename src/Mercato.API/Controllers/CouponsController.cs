using MediatR;
using Mercato.Application.Coupons.Commands.CreateCoupon;
using Mercato.Application.Coupons.Commands.DeleteCoupon;
using Mercato.Application.Coupons.Commands.UpdateCoupon;
using Mercato.Application.Coupons.Queries.GetAllCoupons;
using Mercato.Application.Coupons.Queries.GetCouponById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mercato.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CouponsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllCoupons(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllCouponsQuery(), cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCouponById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCouponByIdQuery(id), cancellationToken);

        if (result is null)
            return NotFound("Coupon not found.");

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCoupon(Guid id, [FromBody] UpdateCouponCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id and body id do not match.");

        var result = await _mediator.Send(command, cancellationToken);

        if (!result)
            return NotFound("Coupon not found.");

        return Ok("Coupon updated successfully.");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCoupon(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCouponCommand(id), cancellationToken);

        if (!result)
            return NotFound("Coupon not found.");

        return Ok("Coupon deleted successfully.");
    }
}