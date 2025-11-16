using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;

	public DebugController(UserManager<ApplicationUser> userManager, IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory)
	{
		_userManager = userManager;
		_claimsFactory = claimsFactory;
	}

	// GET: /api/debug/user-claims?email=admin@example.com
	[AllowAnonymous]
	[HttpGet("user-claims")]
	public async Task<IActionResult> GetUserClaims([FromQuery] string email)
	{
		if (string.IsNullOrEmpty(email)) return BadRequest("Provide email query parameter");

		var user = await _userManager.FindByEmailAsync(email);
		if (user == null) return NotFound();

		// Create ClaimsPrincipal using the registered factory
		var principal = await _claimsFactory.CreateAsync(user);

		var claims = principal.Claims.Select(c => new { c.Type, c.Value }).ToList();
		var roles = await _userManager.GetRolesAsync(user);

		return Ok(new { Email = user.Email, UserName = user.UserName, Roles = roles, Claims = claims });
	}
}
