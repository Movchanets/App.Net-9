using Application.DTOs;
using MediatR;

namespace Application.Queries.Catalog.GetProductBySkuCode;

public sealed record GetProductBySkuCodeQuery(string SkuCode) : IRequest<ServiceResponse<ProductDetailsDto>>;
