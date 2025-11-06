using System.Reflection;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace API.Filters;

/// <summary>
/// Action filter which looks for a Turnstile token in action arguments (property named "TurnstileToken" or
/// parameter names containing "turnstile"/"token") and validates it using <see cref="ITurnstileService"/>.
/// If validation fails, the request is short-circuited with 400 BadRequest.
/// </summary>
public sealed class TurnstileValidationFilter : IAsyncActionFilter
{
	private readonly ITurnstileService _turnstile;
	private readonly ILogger<TurnstileValidationFilter> _logger;

	public TurnstileValidationFilter(ITurnstileService turnstile, ILogger<TurnstileValidationFilter> logger)
	{
		_turnstile = turnstile;
		_logger = logger;
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		try
		{
			var token = ExtractTokenFromArguments(context);
			if (string.IsNullOrWhiteSpace(token))
			{
				// No token present — nothing to validate.
				await next();
				return;
			}

			var remoteIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();
			var ok = await _turnstile.ValidateAsync(token, remoteIp);
			if (!ok)
			{
				_logger.LogWarning("Turnstile validation failed for request {Path}", context.HttpContext.Request.Path);
				context.Result = new BadRequestObjectResult(new { Message = "Turnstile validation failed" });
				return;
			}

			await next();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during Turnstile validation filter");
			context.Result = new StatusCodeResult(500);
		}
	}

	private static string? ExtractTokenFromArguments(ActionExecutingContext context)
	{
		// Check action parameters first: if a string parameter name suggests it's a token, use it.
		foreach (var kv in context.ActionArguments)
		{
			var name = kv.Key ?? string.Empty;
			var value = kv.Value;
			if (value == null) continue;

			// If parameter name contains "turnstile" or "token" and is a string, treat as token
			if ((name.IndexOf("turnstile", StringComparison.OrdinalIgnoreCase) >= 0 ||
				 name.IndexOf("token", StringComparison.OrdinalIgnoreCase) >= 0) && value is string s)
			{
				if (!string.IsNullOrWhiteSpace(s)) return s;
			}

			// If the argument is an object, reflect for a property named TurnstileToken (case-insensitive)
			var type = value.GetType();
			var prop = type.GetProperty("TurnstileToken", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
			if (prop != null && prop.PropertyType == typeof(string))
			{
				var token = prop.GetValue(value) as string;
				if (!string.IsNullOrWhiteSpace(token)) return token;
			}
		}

		return null;
	}
}
