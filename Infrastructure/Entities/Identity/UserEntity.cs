using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Entities;

public class UserEntity : IdentityUser<long> // long як primary key
{

    public bool IsBlocked { get; set; }
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? RefreshToken { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Навігаційна властивість
    public ICollection<UserRoleEntity> UserRoles { get; set; } = new List<UserRoleEntity>();
}