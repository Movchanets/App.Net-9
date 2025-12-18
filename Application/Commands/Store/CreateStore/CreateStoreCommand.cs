using Application.DTOs;
using MediatR;

namespace Application.Commands.Store.CreateStore;

public sealed record CreateStoreCommand(
	Guid UserId,
	string Name,
	string? Description = null
) : IRequest<ServiceResponse<Guid>>;
