using Mercato.Application.Auth.Models;

namespace Mercato.Application.Common.Interfaces;

public interface IAuthService
{
    Task<string> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task<string> ConfirmEmailAsync(Guid userId, string token);
}