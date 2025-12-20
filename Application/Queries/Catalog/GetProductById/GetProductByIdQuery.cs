using Application.DTOs;
using MediatR;

namespace Application.Queries.Catalog.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ServiceResponse<ProductDetailsDto>>;
