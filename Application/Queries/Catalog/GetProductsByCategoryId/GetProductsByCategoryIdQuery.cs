using Application.DTOs;
using MediatR;

namespace Application.Queries.Catalog.GetProductsByCategoryId;

public sealed record GetProductsByCategoryIdQuery(Guid CategoryId)
	: IRequest<ServiceResponse<IReadOnlyList<ProductSummaryDto>>>;
