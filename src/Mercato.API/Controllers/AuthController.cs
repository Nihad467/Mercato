using Mercato.Application.Auth.Models;
using Mercato.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Mercato.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var message = await _authService.RegisterAsync(request);

        return Ok(new
        {
            message
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        return Ok(result);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] Guid userId,
        [FromQuery] string token)
    {
        try
        {
            var message = await _authService.ConfirmEmailAsync(userId, token);

            var html = BuildEmailConfirmationSuccessPage(message);

            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            var html = BuildEmailConfirmationErrorPage(ex.Message);

            return Content(html, "text/html");
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var message = await _authService.ForgotPasswordAsync(request);

        return Ok(new
        {
            message
        });
    }

    [HttpGet("reset-password-page")]
    public IActionResult ResetPasswordPage(
        [FromQuery] string email,
        [FromQuery] string token)
    {
        var html = BuildResetPasswordPage(email, token);

        return Content(html, "text/html");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordRequest request)
    {
        try
        {
            var message = await _authService.ResetPasswordAsync(request);

            var html = BuildPasswordResetSuccessPage(message);

            return Content(html, "text/html");
        }
        catch (Exception ex)
        {
            var html = BuildPasswordResetErrorPage(ex.Message);

            return Content(html, "text/html");
        }
    }

    private static string BuildEmailConfirmationSuccessPage(string message)
    {
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Email Confirmed</title>
</head>
<body style='margin:0; min-height:100vh; background:#f4f7fb; font-family:Arial, Helvetica, sans-serif; display:flex; align-items:center; justify-content:center;'>

    <div style='width:100%; max-width:540px; margin:24px; background:#ffffff; border-radius:24px; box-shadow:0 20px 60px rgba(15, 23, 42, 0.12); overflow:hidden;'>
        <div style='background:linear-gradient(135deg, #111827, #374151); padding:34px 38px; color:#ffffff;'>
            <h1 style='margin:0; font-size:30px; letter-spacing:0.3px;'>Mercato</h1>
            <p style='margin:8px 0 0; color:#d1d5db; font-size:15px;'>Account verification</p>
        </div>

        <div style='padding:42px 38px 36px; text-align:center;'>
            <div style='width:76px; height:76px; border-radius:50%; background:#ecfdf5; margin:0 auto 22px; display:flex; align-items:center; justify-content:center;'>
                <div style='font-size:38px; color:#047857;'>✓</div>
            </div>

            <div style='display:inline-block; padding:8px 14px; background:#ecfdf5; color:#047857; border-radius:999px; font-size:13px; font-weight:700; margin-bottom:18px;'>
                Email confirmed
            </div>

            <h2 style='margin:0 0 12px; color:#111827; font-size:28px;'>
                Your email is verified
            </h2>

            <p style='margin:0 auto; max-width:420px; color:#4b5563; font-size:15px; line-height:1.7;'>
                {message} You can now log in and continue using your Mercato account.
            </p>

            <div style='margin-top:30px; padding:18px; background:#f9fafb; border-radius:16px; color:#6b7280; font-size:13px; line-height:1.6;'>
                This page can be closed safely.
            </div>
        </div>

        <div style='padding:20px 38px; background:#f9fafb; border-top:1px solid #edf2f7; text-align:center;'>
            <p style='margin:0; color:#9ca3af; font-size:12px;'>
                © {DateTime.UtcNow.Year} Mercato. All rights reserved.
            </p>
        </div>
    </div>

</body>
</html>";
    }

    private static string BuildEmailConfirmationErrorPage(string errorMessage)
    {
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Email Confirmation Failed</title>
</head>
<body style='margin:0; min-height:100vh; background:#f4f7fb; font-family:Arial, Helvetica, sans-serif; display:flex; align-items:center; justify-content:center;'>

    <div style='width:100%; max-width:540px; margin:24px; background:#ffffff; border-radius:24px; box-shadow:0 20px 60px rgba(15, 23, 42, 0.12); overflow:hidden;'>
        <div style='background:linear-gradient(135deg, #111827, #374151); padding:34px 38px; color:#ffffff;'>
            <h1 style='margin:0; font-size:30px; letter-spacing:0.3px;'>Mercato</h1>
            <p style='margin:8px 0 0; color:#d1d5db; font-size:15px;'>Account verification</p>
        </div>

        <div style='padding:42px 38px 36px; text-align:center;'>
            <div style='width:76px; height:76px; border-radius:50%; background:#fef2f2; margin:0 auto 22px; display:flex; align-items:center; justify-content:center;'>
                <div style='font-size:38px; color:#dc2626;'>!</div>
            </div>

            <div style='display:inline-block; padding:8px 14px; background:#fef2f2; color:#dc2626; border-radius:999px; font-size:13px; font-weight:700; margin-bottom:18px;'>
                Confirmation failed
            </div>

            <h2 style='margin:0 0 12px; color:#111827; font-size:28px;'>
                We could not verify your email
            </h2>

            <p style='margin:0 auto; max-width:420px; color:#4b5563; font-size:15px; line-height:1.7;'>
                {errorMessage}
            </p>

            <div style='margin-top:30px; padding:18px; background:#f9fafb; border-radius:16px; color:#6b7280; font-size:13px; line-height:1.6;'>
                Please try registering again or request a new confirmation link later.
            </div>
        </div>

        <div style='padding:20px 38px; background:#f9fafb; border-top:1px solid #edf2f7; text-align:center;'>
            <p style='margin:0; color:#9ca3af; font-size:12px;'>
                © {DateTime.UtcNow.Year} Mercato. All rights reserved.
            </p>
        </div>
    </div>

</body>
</html>";
    }

    private static string BuildResetPasswordPage(string email, string token)
    {
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Password</title>
</head>
<body style='margin:0; min-height:100vh; background:#f4f7fb; font-family:Arial, Helvetica, sans-serif; display:flex; align-items:center; justify-content:center;'>

    <div style='width:100%; max-width:560px; margin:24px; background:#ffffff; border-radius:24px; box-shadow:0 20px 60px rgba(15, 23, 42, 0.12); overflow:hidden;'>

        <div style='background:linear-gradient(135deg, #111827, #374151); padding:34px 38px; color:#ffffff;'>
            <h1 style='margin:0; font-size:30px; letter-spacing:0.3px;'>Mercato</h1>
            <p style='margin:8px 0 0; color:#d1d5db; font-size:15px;'>Password reset</p>
        </div>

        <div style='padding:40px 38px;'>
            <div style='display:inline-block; padding:8px 14px; background:#fff7ed; color:#c2410c; border-radius:999px; font-size:13px; font-weight:700; margin-bottom:18px;'>
                Security action
            </div>

            <h2 style='margin:0 0 12px; color:#111827; font-size:28px;'>
                Create a new password
            </h2>

            <p style='margin:0 0 26px; color:#4b5563; font-size:15px; line-height:1.7;'>
                Enter your new password below. After saving, you can log in with the new password.
            </p>

            <form method='post' action='/api/Auth/reset-password'>
                <input type='hidden' name='Email' value='{email}' />
                <input type='hidden' name='Token' value='{token}' />

                <label style='display:block; color:#374151; font-size:13px; font-weight:700; margin-bottom:8px;'>
                    New password
                </label>

                <input type='password'
                       name='NewPassword'
                       required
                       minlength='6'
                       placeholder='Enter new password'
                       style='width:100%; box-sizing:border-box; padding:14px 16px; border:1px solid #d1d5db; border-radius:12px; font-size:15px; outline:none;' />

                <button type='submit'
                        style='width:100%; margin-top:22px; background:#111827; color:#ffffff; border:none; padding:14px 18px; border-radius:12px; font-size:15px; font-weight:700; cursor:pointer;'>
                    Reset Password
                </button>
            </form>

            <div style='margin-top:26px; padding:16px; background:#f9fafb; border-radius:14px; color:#6b7280; font-size:13px; line-height:1.6;'>
                If you did not request this, you can close this page safely.
            </div>
        </div>

        <div style='padding:20px 38px; background:#f9fafb; border-top:1px solid #edf2f7; text-align:center;'>
            <p style='margin:0; color:#9ca3af; font-size:12px;'>
                © {DateTime.UtcNow.Year} Mercato. All rights reserved.
            </p>
        </div>

    </div>

</body>
</html>";
    }

    private static string BuildPasswordResetSuccessPage(string message)
    {
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Password Reset Successful</title>
</head>
<body style='margin:0; min-height:100vh; background:#f4f7fb; font-family:Arial, Helvetica, sans-serif; display:flex; align-items:center; justify-content:center;'>

    <div style='width:100%; max-width:540px; margin:24px; background:#ffffff; border-radius:24px; box-shadow:0 20px 60px rgba(15, 23, 42, 0.12); overflow:hidden;'>
        <div style='background:linear-gradient(135deg, #111827, #374151); padding:34px 38px; color:#ffffff;'>
            <h1 style='margin:0; font-size:30px; letter-spacing:0.3px;'>Mercato</h1>
            <p style='margin:8px 0 0; color:#d1d5db; font-size:15px;'>Password reset</p>
        </div>

        <div style='padding:42px 38px 36px; text-align:center;'>
            <div style='width:76px; height:76px; border-radius:50%; background:#ecfdf5; margin:0 auto 22px; display:flex; align-items:center; justify-content:center;'>
                <div style='font-size:38px; color:#047857;'>✓</div>
            </div>

            <div style='display:inline-block; padding:8px 14px; background:#ecfdf5; color:#047857; border-radius:999px; font-size:13px; font-weight:700; margin-bottom:18px;'>
                Password updated
            </div>

            <h2 style='margin:0 0 12px; color:#111827; font-size:28px;'>
                Your password has been reset
            </h2>

            <p style='margin:0 auto; max-width:420px; color:#4b5563; font-size:15px; line-height:1.7;'>
                {message} You can now log in with your new password.
            </p>
        </div>

        <div style='padding:20px 38px; background:#f9fafb; border-top:1px solid #edf2f7; text-align:center;'>
            <p style='margin:0; color:#9ca3af; font-size:12px;'>
                © {DateTime.UtcNow.Year} Mercato. All rights reserved.
            </p>
        </div>
    </div>

</body>
</html>";
    }

    private static string BuildPasswordResetErrorPage(string errorMessage)
    {
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Password Reset Failed</title>
</head>
<body style='margin:0; min-height:100vh; background:#f4f7fb; font-family:Arial, Helvetica, sans-serif; display:flex; align-items:center; justify-content:center;'>

    <div style='width:100%; max-width:540px; margin:24px; background:#ffffff; border-radius:24px; box-shadow:0 20px 60px rgba(15, 23, 42, 0.12); overflow:hidden;'>
        <div style='background:linear-gradient(135deg, #111827, #374151); padding:34px 38px; color:#ffffff;'>
            <h1 style='margin:0; font-size:30px; letter-spacing:0.3px;'>Mercato</h1>
            <p style='margin:8px 0 0; color:#d1d5db; font-size:15px;'>Password reset</p>
        </div>

        <div style='padding:42px 38px 36px; text-align:center;'>
            <div style='width:76px; height:76px; border-radius:50%; background:#fef2f2; margin:0 auto 22px; display:flex; align-items:center; justify-content:center;'>
                <div style='font-size:38px; color:#dc2626;'>!</div>
            </div>

            <div style='display:inline-block; padding:8px 14px; background:#fef2f2; color:#dc2626; border-radius:999px; font-size:13px; font-weight:700; margin-bottom:18px;'>
                Reset failed
            </div>

            <h2 style='margin:0 0 12px; color:#111827; font-size:28px;'>
                We could not reset your password
            </h2>

            <p style='margin:0 auto; max-width:420px; color:#4b5563; font-size:15px; line-height:1.7;'>
                {errorMessage}
            </p>
        </div>

        <div style='padding:20px 38px; background:#f9fafb; border-top:1px solid #edf2f7; text-align:center;'>
            <p style='margin:0; color:#9ca3af; font-size:12px;'>
                © {DateTime.UtcNow.Year} Mercato. All rights reserved.
            </p>
        </div>
    </div>

</body>
</html>";
    }
}