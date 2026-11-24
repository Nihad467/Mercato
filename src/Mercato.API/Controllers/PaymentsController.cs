using MediatR;
using Mercato.Application.Payments.Commands.CreatePayment;
using Mercato.Application.Payments.Commands.HandleStripeWebhook;
using Mercato.Application.Payments.Commands.MockPaymentFail;
using Mercato.Application.Payments.Commands.MockPaymentSuccess;
using Mercato.Application.Payments.Queries.GetPaymentByOrderId;
using Mercato.Application.Options;
using Mercato.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using System.Text;

namespace Mercato.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly StripeOptions _stripeOptions;
    private readonly ILogger<PaymentsController> _logger;
    private readonly IWebHostEnvironment _environment;

    public PaymentsController(
        IMediator mediator,
        IOptions<StripeOptions> stripeOptions,
        ILogger<PaymentsController> logger,
        IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _stripeOptions = stripeOptions.Value;
        _logger = logger;
        _environment = environment;
    }

    [HttpGet("stripe/availability")]
    public IActionResult GetStripeAvailability()
    {
        var message = GetStripeConfigurationMessage();
        return Ok(new { ready = message is null, message });
    }

    [HttpPost("create/{orderId}")]
    public async Task<IActionResult> Create(int orderId, [FromQuery] PaymentProvider provider = PaymentProvider.Stripe)
    {
        if (provider == PaymentProvider.Stripe && GetStripeConfigurationMessage() is { } configurationMessage)
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = configurationMessage });

        try
        {
            var result = await _mediator.Send(new CreatePaymentCommand(orderId, provider), HttpContext.RequestAborted);
            return Ok(result);
        }
        catch (StripeException exception)
        {
            _logger.LogError(exception, "Stripe could not create a Checkout Session for order {OrderId}.", orderId);
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "Stripe Checkout səhifəsi yaradıla bilmədi. Stripe test açarını və valyuta ayarını yoxlayın."
            });
        }
    }

    [HttpPost("mock/success/{orderId}")]
    public async Task<IActionResult> MockSuccess(int orderId)
    {
        if (!_environment.IsDevelopment()) return NotFound();
        var result = await _mediator.Send(new MockPaymentSuccessCommand(orderId));
        return Ok(new { message = result });
    }

    [HttpPost("mock/fail/{orderId}")]
    public async Task<IActionResult> MockFail(int orderId, [FromQuery] string? reason = null)
    {
        if (!_environment.IsDevelopment()) return NotFound();
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
        if (!_stripeOptions.HasValidWebhookSecret)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Stripe Webhook Signing Secret konfiqurasiya edilməyib."
            });
        }

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

    private string? GetStripeConfigurationMessage()
    {
        if (!_stripeOptions.HasValidSecretKey)
            return "Stripe test Secret Key konfiqurasiya edilməyib. Açarı tətbiqin User Secrets bölməsinə əlavə edin.";

        if (!_stripeOptions.HasValidWebhookSecret)
            return "Stripe Webhook Signing Secret konfiqurasiya edilməyib. Stripe CLI ilə webhook-u lokal API-yə yönləndirin.";

        if (!_stripeOptions.HasValidReturnUrls)
            return "Stripe success/cancel URL-ləri etibarlı HTTPS və ya localhost URL olmalıdır.";

        return null;
    }
}
