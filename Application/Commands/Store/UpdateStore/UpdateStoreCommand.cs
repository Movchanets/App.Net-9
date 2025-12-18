using Application.DTOs;
using MediatR;

namespace Application.Commands.Store.UpdateStore;

public sealed record UpdateStoreCommand(
	Guid UserId,
	string Name,
	string? Description = null
) : IRequest<ServiceResponse>;
