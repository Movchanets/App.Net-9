using Application.DTOs;
using MediatR;

namespace Application.Commands.Product.Sku.DeleteSku;

public sealed record DeleteSkuCommand(
	Guid UserId,
	Guid ProductId,
	Guid SkuId
) : IRequest<ServiceResponse>;
