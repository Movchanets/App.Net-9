using System;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Entities.Identity;

/// <summary>
/// Infrastructure модель користувача для ASP.NET Core Identity
/// Містить тільки Infrastructure-специфічні поля (токени, authentication)
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
	/// <summary>
	/// Refresh token для JWT аутентифікації
	/// </summary>
	public string? RefreshToken { get; set; }

	/// <summary>
	/// Час закінчення дії refresh token
	/// </summary>
	public DateTime? RefreshTokenExpiryTime { get; set; }

	/// <summary>
	/// Зв'язок з чистим доменним User
	/// </summary>
	public Guid? DomainUserId { get; set; }

	/// <summary>
	/// Навігаційна властивість до доменного User
	/// </summary>
	public virtual User? DomainUser { get; set; }

	/// <summary>
	/// Навігаційна властивість для Identity ролей
	/// </summary>
	public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
}
