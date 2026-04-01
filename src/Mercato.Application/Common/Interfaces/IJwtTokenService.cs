namespace Mercato.Application.Common.Interfaces;

public interface IJwtTokenService
{
    Task<(string Token, DateTime ExpiresAtUtc)> CreateAccessTokenAsync(
        string userId,
        string email,
        IList<string> roles);
}