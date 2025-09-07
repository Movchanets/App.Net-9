using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Entities;

public class RoleEntity : IdentityRole<long>
{
    public string Description { get; set; } = string.Empty;

    // Навігаційна властивість
    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}