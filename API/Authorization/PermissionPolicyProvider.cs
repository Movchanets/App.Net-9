using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace API.Authorization;

/// <summary>
/// Dynamic policy provider that recognizes policies in the form "Permission:{permission_name}".
/// It creates an <see cref="AuthorizationPolicy"/> with a <see cref="PermissionRequirement"/> for the requested permission.
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
	private const string Prefix = "Permission:";
	private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;

	public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
	{
		_fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
	}

	// Default policy must be non-nullable in current target frameworks
	public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackProvider.GetDefaultPolicyAsync();

	// Fallback and GetPolicy may be nullable depending on framework; keep them nullable to match the interface in newer runtimes
	public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackProvider.GetFallbackPolicyAsync();

	public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
	{
		if (policyName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
		{
			var permission = policyName.Substring(Prefix.Length);
			var policy = new AuthorizationPolicyBuilder();
			policy.AddRequirements(new PermissionRequirement(permission));
			return Task.FromResult<AuthorizationPolicy?>(policy.Build());
		}

		return _fallbackProvider.GetPolicyAsync(policyName);
	}
}
