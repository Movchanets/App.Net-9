using MediatR;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Entities;
using Application.Interfaces;
using Application.Services.Email;

namespace Application.Commands.User.SendTestEmail;

public sealed class SendTestEmailHandler(IEmailQueue emailQueue) : IRequestHandler<SendTestEmailCommand>
{
	public async Task Handle(SendTestEmailCommand request, CancellationToken cancellationToken)
	{
		var callbackUrl = $"{request.Origin}/test-email?to={System.Net.WebUtility.UrlEncode(request.Email)}&id={System.Guid.NewGuid()}";
		await emailQueue.EnqueueEmailAsync(request.Email, callbackUrl);
		return;
	}
}
