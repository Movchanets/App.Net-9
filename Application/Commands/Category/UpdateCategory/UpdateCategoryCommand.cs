using Application.DTOs;
using MediatR;

namespace Application.Commands.Category.UpdateCategory;

public sealed record UpdateCategoryCommand(
	Guid Id,
	string Name,
	string? Description = null,
	Guid? ParentCategoryId = null
) : IRequest<ServiceResponse>;
