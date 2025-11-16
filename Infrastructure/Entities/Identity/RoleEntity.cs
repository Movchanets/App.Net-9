using System;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Entities.Identity;

public class RoleEntity : IdentityRole<Guid>
{
    public string Description { get; set; } = string.Empty;

    // Навігаційна властивість
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
}
