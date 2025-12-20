using Application.DTOs;
using MediatR;

namespace Application.Queries.Catalog.GetCategories;

public sealed record GetCategoriesQuery(Guid? ParentCategoryId = null, bool TopLevelOnly = false)
	: IRequest<ServiceResponse<IReadOnlyList<CategoryDto>>>;
