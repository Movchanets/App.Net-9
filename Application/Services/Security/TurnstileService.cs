using System.Net.Http.Json;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Services.Security;

public sealed class TurnstileService(HttpClient http, IConfiguration config, ILogger<TurnstileService> logger)
	: ITurnstileService
{
	private readonly string? _secret = config["Turnstile:Secret"];

	public async Task<bool> ValidateAsync(string token, string? remoteIp = null)
	{
		if (string.IsNullOrWhiteSpace(_secret))
		{
			logger.LogWarning("Turnstile secret is not configured. Failing validation for safety.");
			return false;
		}

		using var content = new FormUrlEncodedContent(new Dictionary<string, string?>
		{
			["secret"] = _secret,
			["response"] = token,
			["remoteip"] = remoteIp
		});

		try
		{
			var res = await http.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", content);
			if (!res.IsSuccessStatusCode)
			{
				logger.LogWarning("Turnstile verification returned non-success status {Status}", res.StatusCode);
				return false;
			}

			var body = await res.Content.ReadFromJsonAsync<TurnstileResponse?>();
			if (body == null)
			{
				logger.LogWarning("Turnstile verification returned empty body");
				return false;
			}

			if (!body.Success)
			{
				logger.LogInformation("Turnstile validation failed: {Errors}", string.Join(',', body.ErrorCodes ?? Array.Empty<string>()));
			}
			logger.LogInformation("Turnstile validation succeeded: {Success}", body.Success);
			return body.Success;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Turnstile validation request failed");
			return false;
		}
	}

	private sealed class TurnstileResponse
	{
		public bool Success { get; set; }
		public string? Challenge_ts { get; set; }
		public string? Hostname { get; set; }
		public string[]? ErrorCodes { get; set; }
	}
}
