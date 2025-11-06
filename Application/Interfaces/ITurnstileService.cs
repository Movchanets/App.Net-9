namespace Application.Interfaces;

public interface ITurnstileService
{
	/// <summary>
	/// Validate Cloudflare Turnstile token with Cloudflare API.
	/// </summary>
	/// <param name="token">The token returned from the client widget (cf-turnstile-response).</param>
	/// <param name="remoteIp">Optional remote IP of the user.</param>
	Task<bool> ValidateAsync(string token, string? remoteIp = null);
}
