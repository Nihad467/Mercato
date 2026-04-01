namespace Mercato.Application.Auth.Models;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = default!;
}