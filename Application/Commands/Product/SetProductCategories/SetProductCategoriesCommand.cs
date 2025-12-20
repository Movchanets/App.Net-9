using Application.DTOs;
using MediatR;

namespace Application.Commands.Product.SetProductCategories;

public sealed record SetProductCategoriesCommand(
	Guid UserId,
	Guid ProductId,
	List<Guid> CategoryIds
) : IRequest<ServiceResponse>;
