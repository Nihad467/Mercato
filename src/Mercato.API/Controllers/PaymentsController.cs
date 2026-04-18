using MediatR;
using Mercato.Application.Payments.Commands.CreatePayment;
using Mercato.Application.Payments.Commands.HandleStripeWebhook;
using Mercato.Application.Payments.Commands.MockPaymentFail;
using Mercato.Application.Payments.Commands.MockPaymentSuccess;
using Mercato.Application.Payments.Queries.GetPaymentByOrderId;
using Mercato.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Mercato.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create/{orderId}")]
    public async Task<IActionResult> Create(int orderId, [FromQuery] PaymentProvider provider = PaymentProvider.Mock)
    {
        var result = await _mediator.Send(new CreatePaymentCommand(orderId, provider));
        return Ok(result);
    }

    [HttpPost("mock/success/{orderId}")]
    public async Task<IActionResult> MockSuccess(int orderId)
    {
        var result = await _mediator.Send(new MockPaymentSuccessCommand(orderId));
        return Ok(new { message = result });
    }

    [HttpPost("mock/fail/{orderId}")]
    public async Task<IActionResult> MockFail(int orderId, [FromQuery] string? reason = null)
    {
        var result = await _mediator.Send(new MockPaymentFailCommand(orderId, reason));
        return Ok(new { message = result });
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrderId(int orderId)
    {
        var result = await _mediator.Send(new GetPaymentByOrderIdQuery(orderId));
        return Ok(result);
    }

    [HttpPost("stripe/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook()
    {
        Request.EnableBuffering();

        string payload;

        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            payload = await reader.ReadToEndAsync();
            Request.Body.Position = 0;
        }

        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();

        var result = await _mediator.Send(new HandleStripeWebhookCommand(payload, signatureHeader));

        return Ok(new { message = result });
    }
}