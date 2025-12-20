using Application.DTOs;
using MediatR;

namespace Application.Commands.Product.UpdateProductDetails;

public sealed record UpdateProductDetailsCommand(
	Guid UserId,
	Guid ProductId,
	string Name,
	string? Description
) : IRequest<ServiceResponse>;
