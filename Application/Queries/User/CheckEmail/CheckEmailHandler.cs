using Application.DTOs;
using MediatR;
using Application.Interfaces;

namespace Application.Queries.User.CheckEmail;

public sealed class CheckEmailHandler : IRequestHandler<CheckEmailQuery, CheckEmailResponse>
{
	private readonly IUserService _identity;

	public CheckEmailHandler(IUserService identity)
	{
		_identity = identity;
	}

	public async Task<CheckEmailResponse> Handle(CheckEmailQuery request, CancellationToken cancellationToken)
	{
		var exists = await _identity.EmailExistsAsync(request.Email);
		return new CheckEmailResponse(exists);
	}
}
