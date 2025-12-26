using Application.DTOs;
using MediatR;

namespace Application.Queries.Catalog.GetCategoryBySlug;

public sealed record GetCategoryBySlugQuery(string Slug)
	: IRequest<ServiceResponse<CategoryDto>>;
