using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace API.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
	private readonly ILogger<PermissionAuthorizationHandler> _logger;

	public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
	{
		_logger = logger;
	}

	protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
	{
		// Basic auth check
		if (context.User?.Identity is not { IsAuthenticated: true })
		{
			_logger.LogDebug("Permission check skipped: user is not authenticated (permission={Permission})", requirement.Permission);
			return Task.CompletedTask;
		}

		// Get useful identifiers for logging (do not log tokens)
		var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
					 ?? context.User.FindFirst("sub")?.Value
					 ?? "<unknown>";
		var email = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "<unknown>";

		// Only consider claims with Type == "permission"
		var permissionClaims = context.User.Claims
			.Where(c => string.Equals(c.Type, "permission", System.StringComparison.OrdinalIgnoreCase))
			.Select(c => c.Value)
			.ToArray();

		var hasPermission = permissionClaims.Contains(requirement.Permission);

		_logger.LogInformation("Checking permission '{Permission}' for user {UserId} (email={Email}). PermissionClaims={PermissionClaims}. Result={Result}",
			requirement.Permission,
			userId,
			email,
			permissionClaims,
			hasPermission ? "Allowed" : "Denied");

		if (hasPermission)
		{
			context.Succeed(requirement);
		}

		return Task.CompletedTask;
	}
}
