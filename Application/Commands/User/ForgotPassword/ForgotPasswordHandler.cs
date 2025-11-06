using Application.Commands.User.ForgotPassword;
using Application.Interfaces;
using Application.Services.Email;
using Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Commands.User.ForgotPassword;

public sealed class ForgotPasswordHandler(
	UserManager<UserEntity> userManager,
	IMemoryCache memoryCache,
	IEmailQueue emailQueue)
	: IRequestHandler<ForgotPasswordCommand>
{
	public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
	{
		var user = await userManager.FindByEmailAsync(request.Email);
		if (user == null)
			return;

		var cacheKey = $"forgot:{request.Email}";
		var attempts = memoryCache.GetOrCreate(cacheKey, entry =>
		{
			entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
			return 0;
		});

		if (attempts >= 5)
		{
			await userManager.UpdateSecurityStampAsync(user);
			return;
		}

		memoryCache.Set(cacheKey, attempts + 1, TimeSpan.FromHours(1));

		var token = await userManager.GeneratePasswordResetTokenAsync(user);

		var callbackUrl = $"{request.Origin}/reset-password?email={System.Net.WebUtility.UrlEncode(request.Email)}&token={System.Net.WebUtility.UrlEncode(token)}";

		await emailQueue.EnqueueEmailAsync(request.Email, callbackUrl);

		return;
	}
}
