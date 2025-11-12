using System.Security.Claims;
using System.Threading.Tasks;
using API.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace Application.Tests.Authorization;

public class PermissionAuthorizationHandlerTests
{
	[Fact]
	public async Task Handler_Succeeds_When_Permission_Claim_Present()
	{
		// Arrange
		var requirement = new PermissionRequirement("users.read");
		var handler = new PermissionAuthorizationHandler(NullLogger<PermissionAuthorizationHandler>.Instance);

		var identity = new ClaimsIdentity(new[] { new Claim("permission", "users.read") }, "test");
		var user = new ClaimsPrincipal(identity);

		var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

		// Act
		await handler.HandleAsync(context);

		// Assert
		context.HasSucceeded.Should().BeTrue();
	}

	[Fact]
	public async Task Handler_DoesNot_Succeed_When_Permission_Claim_Missing()
	{
		// Arrange
		var requirement = new PermissionRequirement("users.read");
		var handler = new PermissionAuthorizationHandler(NullLogger<PermissionAuthorizationHandler>.Instance);

		var identity = new ClaimsIdentity(new[] { new Claim("permission", "other.permission") }, "test");
		var user = new ClaimsPrincipal(identity);

		var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

		// Act
		await handler.HandleAsync(context);

		// Assert
		context.HasSucceeded.Should().BeFalse();
	}
}
