using Application.DTOs;
using MediatR;

namespace Application.Commands.Product.SetProductTags;

public sealed record SetProductTagsCommand(
	Guid UserId,
	Guid ProductId,
	List<Guid> TagIds
) : IRequest<ServiceResponse>;
