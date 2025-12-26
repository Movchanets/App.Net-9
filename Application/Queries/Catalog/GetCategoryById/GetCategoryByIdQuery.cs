using Application.DTOs;
using MediatR;

namespace Application.Queries.Catalog.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id)
	: IRequest<ServiceResponse<CategoryDto>>;
