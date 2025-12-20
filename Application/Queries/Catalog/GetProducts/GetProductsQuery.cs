using Application.DTOs;
using MediatR;

namespace Application.Queries.Catalog.GetProducts;

public sealed record GetProductsQuery() : IRequest<ServiceResponse<IReadOnlyList<ProductSummaryDto>>>;
