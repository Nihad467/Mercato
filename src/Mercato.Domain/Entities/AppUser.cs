using Microsoft.AspNetCore.Identity;

namespace Mercato.Domain.Entities;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = default!;
}