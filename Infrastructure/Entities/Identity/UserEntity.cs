using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Entities;

public class UserEntity : IdentityUser<long> // int як primary key
{
    public string FullName { get; set; } = string.Empty;

    // Навігаційна властивість
    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}