using Application.DTOs;
using MediatR;

namespace Application.Commands.Product.SetProductBaseImage;

public sealed record SetProductBaseImageCommand(
	Guid UserId,
	Guid ProductId,
	string? BaseImageUrl
) : IRequest<ServiceResponse>;
