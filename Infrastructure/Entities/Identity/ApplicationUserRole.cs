using System;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Entities.Identity;

/// <summary>
/// Many-to-many зв'язок між ApplicationUser та RoleEntity (Identity)
/// </summary>
public class ApplicationUserRole : IdentityUserRole<Guid>
{
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual RoleEntity Role { get; set; } = null!;
}
