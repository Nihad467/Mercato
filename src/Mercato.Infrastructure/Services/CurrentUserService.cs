using System.Security.Claims;
using Mercato.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Mercato.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdValue = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdValue))
                return null;

            return Guid.TryParse(userIdValue, out var userId) ? userId : null;
        }
    }
}