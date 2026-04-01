namespace Mercato.Application.Auth.Models;

public class AuthResponse
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public DateTime AccessTokenExpiresAtUtc { get; set; }
}