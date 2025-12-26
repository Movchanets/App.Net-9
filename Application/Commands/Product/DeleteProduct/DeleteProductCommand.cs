using Application.DTOs;
using MediatR;

namespace Application.Commands.Product.DeleteProduct;

public sealed record DeleteProductCommand(Guid UserId, Guid ProductId) : IRequest<ServiceResponse>;
