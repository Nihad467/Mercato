using Mercato.Application.Auth.Models;
using Mercato.Application.Common.Constants;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Options;
using Mercato.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net;

namespace Mercato.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly JwtOptions _jwtOptions;
    private readonly EmailOptions _emailOptions;

    public AuthService(
        UserManager<AppUser> userManager,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context,
        IEmailService emailService,
        IOptions<JwtOptions> jwtOptions,
        IOptions<EmailOptions> emailOptions)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _context = context;
        _emailService = emailService;
        _jwtOptions = jwtOptions.Value;
        _emailOptions = emailOptions.Value;
    }

    public async Task<string> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
            throw new Exception("Bu email artıq mövcuddur.");

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            UserName = request.Email,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));

        await _userManager.AddToRoleAsync(user, Roles.Customer);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var confirmationLink =
            $"{_emailOptions.ConfirmationBaseUrl}?userId={user.Id}&token={WebUtility.UrlEncode(token)}";

        var emailBody = BuildEmailConfirmationBody(user.FullName, confirmationLink);

        await _emailService.SendEmailAsync(
            user.Email!,
            "Confirm your Mercato account",
            emailBody);

        return "Qeydiyyat uğurludur. Email təsdiqləmə linki göndərildi.";
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
            throw new Exception("Email və ya şifrə yanlışdır.");

        var passwordCorrect = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordCorrect)
            throw new Exception("Email və ya şifrə yanlışdır.");

        var roles = await _userManager.GetRolesAsync(user);

        var (accessToken, expiresAtUtc) = await _jwtTokenService.CreateAccessTokenAsync(
            user.Id.ToString(),
            user.Email ?? string.Empty,
            roles);

        var refreshToken = await CreateRefreshTokenAsync(user.Id, CancellationToken.None);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = expiresAtUtc
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.GetRefreshTokenAsync(refreshToken, CancellationToken.None);

        if (storedToken is null)
            throw new Exception("Refresh token tapılmadı.");

        if (storedToken.IsRevoked)
            throw new Exception("Refresh token deaktiv edilib.");

        if (storedToken.ExpiresAtUtc <= DateTime.UtcNow)
            throw new Exception("Refresh token vaxtı bitib.");

        var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());

        if (user is null)
            throw new Exception("User tapılmadı.");

        storedToken.IsRevoked = true;
        await _context.SaveChangesAsync(CancellationToken.None);

        var roles = await _userManager.GetRolesAsync(user);

        var (accessToken, expiresAtUtc) = await _jwtTokenService.CreateAccessTokenAsync(
            user.Id.ToString(),
            user.Email ?? string.Empty,
            roles);

        var newRefreshToken = await CreateRefreshTokenAsync(user.Id, CancellationToken.None);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAtUtc = expiresAtUtc
        };
    }

    public async Task<string> ConfirmEmailAsync(Guid userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            throw new Exception("User tapılmadı.");

        if (user.EmailConfirmed)
            return "Email artıq təsdiqlənib.";

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));

        return "Email uğurla təsdiqləndi.";
    }

    public async Task<string> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return "Əgər bu email sistemdə varsa, password reset linki göndərildi.";

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var resetLink =
            $"{_emailOptions.PasswordResetBaseUrl}?email={WebUtility.UrlEncode(user.Email)}&token={WebUtility.UrlEncode(token)}";

        var emailBody = BuildPasswordResetEmailBody(user.FullName, resetLink);

        await _emailService.SendEmailAsync(
            user.Email!,
            "Reset your Mercato password",
            emailBody);

        return "Əgər bu email sistemdə varsa, password reset linki göndərildi.";
    }

    public async Task<string> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
            throw new Exception("User tapılmadı.");

        var result = await _userManager.ResetPasswordAsync(
            user,
            request.Token,
            request.NewPassword);

        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));

        return "Şifrə uğurla yeniləndi.";
    }

    private async Task<string> CreateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = $"{Guid.NewGuid():N}{Guid.NewGuid():N}",
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            IsRevoked = false
        };

        await _context.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return refreshToken.Token;
    }

    private static string BuildEmailConfirmationBody(string fullName, string confirmationLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Confirm Email</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f4f7fb; font-family: Arial, Helvetica, sans-serif;'>

    <div style='width: 100%; background-color: #f4f7fb; padding: 32px 0;'>
        <div style='max-width: 620px; margin: 0 auto; background-color: #ffffff; border-radius: 18px; overflow: hidden; box-shadow: 0 10px 30px rgba(15, 23, 42, 0.08);'>

            <div style='background: linear-gradient(135deg, #111827, #374151); padding: 30px 36px; color: #ffffff;'>
                <h1 style='margin: 0; font-size: 28px; letter-spacing: 0.3px;'>Mercato</h1>
                <p style='margin: 8px 0 0; font-size: 15px; color: #d1d5db;'>
                    Confirm your email address
                </p>
            </div>

            <div style='padding: 34px 36px 28px;'>
                <div style='display: inline-block; padding: 8px 14px; background-color: #eff6ff; color: #1d4ed8; border-radius: 999px; font-size: 13px; font-weight: 700;'>
                    Account verification
                </div>

                <h2 style='margin: 22px 0 10px; font-size: 24px; color: #111827;'>
                    Welcome to Mercato, {fullName}!
                </h2>

                <p style='margin: 0; font-size: 15px; line-height: 1.7; color: #4b5563;'>
                    Please confirm your email address to activate your account and start using Mercato.
                </p>

                <div style='margin: 30px 0; text-align: center;'>
                    <a href='{confirmationLink}'
                       style='display: inline-block; background-color: #111827; color: #ffffff; text-decoration: none; padding: 14px 26px; border-radius: 12px; font-size: 15px; font-weight: 700;'>
                        Confirm Email
                    </a>
                </div>

                <p style='margin: 0; font-size: 13px; line-height: 1.6; color: #6b7280;'>
                    If the button does not work, copy and paste this link into your browser:
                </p>

                <p style='word-break: break-all; font-size: 12px; color: #2563eb; line-height: 1.6; margin-top: 8px;'>
                    {confirmationLink}
                </p>

                <div style='margin-top: 26px; padding: 16px; background-color: #f9fafb; border-radius: 14px;'>
                    <p style='margin: 0; color: #6b7280; font-size: 13px; line-height: 1.6;'>
                        If you did not create this account, you can safely ignore this email.
                    </p>
                </div>
            </div>

            <div style='padding: 22px 36px; background-color: #f9fafb; border-top: 1px solid #edf2f7;'>
                <p style='margin: 0; color: #9ca3af; font-size: 12px;'>
                    © {DateTime.UtcNow.Year} Mercato. All rights reserved.
                </p>
            </div>

        </div>
    </div>

</body>
</html>";
    }

    private static string BuildPasswordResetEmailBody(string fullName, string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Password</title>
</head>
<body style='margin: 0; padding: 0; background-color: #f4f7fb; font-family: Arial, Helvetica, sans-serif;'>

    <div style='width: 100%; background-color: #f4f7fb; padding: 32px 0;'>
        <div style='max-width: 620px; margin: 0 auto; background-color: #ffffff; border-radius: 18px; overflow: hidden; box-shadow: 0 10px 30px rgba(15, 23, 42, 0.08);'>

            <div style='background: linear-gradient(135deg, #111827, #374151); padding: 30px 36px; color: #ffffff;'>
                <h1 style='margin: 0; font-size: 28px; letter-spacing: 0.3px;'>Mercato</h1>
                <p style='margin: 8px 0 0; font-size: 15px; color: #d1d5db;'>
                    Password reset request
                </p>
            </div>

            <div style='padding: 34px 36px 28px;'>
                <div style='display: inline-block; padding: 8px 14px; background-color: #fff7ed; color: #c2410c; border-radius: 999px; font-size: 13px; font-weight: 700;'>
                    Security action
                </div>

                <h2 style='margin: 22px 0 10px; font-size: 24px; color: #111827;'>
                    Reset your password, {fullName}
                </h2>

                <p style='margin: 0; font-size: 15px; line-height: 1.7; color: #4b5563;'>
                    We received a request to reset your Mercato account password. Click the button below to create a new password.
                </p>

                <div style='margin: 30px 0; text-align: center;'>
                    <a href='{resetLink}'
                       style='display: inline-block; background-color: #111827; color: #ffffff; text-decoration: none; padding: 14px 26px; border-radius: 12px; font-size: 15px; font-weight: 700;'>
                        Reset Password
                    </a>
                </div>

                <p style='margin: 0; font-size: 13px; line-height: 1.6; color: #6b7280;'>
                    If the button does not work, copy and paste this link into your browser:
                </p>

                <p style='word-break: break-all; font-size: 12px; color: #2563eb; line-height: 1.6; margin-top: 8px;'>
                    {resetLink}
                </p>

                <div style='margin-top: 26px; padding: 16px; background-color: #f9fafb; border-radius: 14px;'>
                    <p style='margin: 0; color: #6b7280; font-size: 13px; line-height: 1.6;'>
                        If you did not request a password reset, you can safely ignore this email.
                    </p>
                </div>
            </div>

            <div style='padding: 22px 36px; background-color: #f9fafb; border-top: 1px solid #edf2f7;'>
                <p style='margin: 0; color: #9ca3af; font-size: 12px;'>
                    © {DateTime.UtcNow.Year} Mercato. All rights reserved.
                </p>
            </div>

        </div>
    </div>

</body>
</html>";
    }
}