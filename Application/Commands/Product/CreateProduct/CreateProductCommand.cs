using Application.DTOs;
using MediatR;

namespace Application.Commands.Product.CreateProduct;

public sealed record CreateProductCommand(
	Guid UserId,
	string Name,
	string? Description,
	Guid CategoryId,
	decimal Price,
	int StockQuantity,
	Dictionary<string, object?>? Attributes = null,
	List<Guid>? TagIds = null
) : IRequest<ServiceResponse<Guid>>;
