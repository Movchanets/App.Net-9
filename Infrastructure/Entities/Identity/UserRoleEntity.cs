using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Entities;

public class UserRoleEntity : IdentityUserRole<long>
{
    public virtual UserEntity User { get; set; } = null!;
    public virtual RoleEntity Role { get; set; } = null!;
}