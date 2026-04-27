using Mercato.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mercato.API.Controllers;

[ApiController]
[Route("api/test-email")]
public class TestEmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public TestEmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> SendTestEmail(
        [FromQuery] string to,
        CancellationToken cancellationToken)
    {
        try
        {
            await _emailService.SendEmailAsync(
                to,
                "Mercato test email",
                "<h2>Mercato email service işləyir ✅</h2><p>Bu test emailidir.</p>",
                cancellationToken);

            return Ok("Email sent successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = ex.Message,
                innerError = ex.InnerException?.Message
            });
        }
    }
}