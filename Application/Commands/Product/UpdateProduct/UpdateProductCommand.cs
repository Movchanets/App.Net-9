using Application.DTOs;
using MediatR;

namespace Application.Commands.Product.UpdateProduct;

public sealed record UpdateProductCommand(
	Guid UserId,
	Guid ProductId,
	string Name,
	string? Description,
	Guid CategoryId,
	List<Guid>? TagIds = null
) : IRequest<ServiceResponse>;
