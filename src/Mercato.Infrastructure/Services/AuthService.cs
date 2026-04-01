using Mercato.Application.Auth.Models;
using Mercato.Application.Common.Constants;
using Mercato.Application.Common.Interfaces;
using Mercato.Application.Options;
using Mercato.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Mercato.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<AppUser> userManager,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context,
        IEmailService emailService,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _context = context;
        _emailService = emailService;
        _jwtOptions = jwtOptions.Value;
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
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(x => x.Description)));

        await _userManager.AddToRoleAsync(user, Roles.Customer);

        return "Qeydiyyat uğurludur.";
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

    public Task<string> ConfirmEmailAsync(Guid userId, string token)
    {
        return Task.FromResult("Email confirmation hələ aktiv deyil.");
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
}