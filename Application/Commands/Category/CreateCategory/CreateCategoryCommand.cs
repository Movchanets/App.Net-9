using Application.DTOs;
using MediatR;

namespace Application.Commands.Category.CreateCategory;

public sealed record CreateCategoryCommand(
	string Name,
	string? Description = null,
	Guid? ParentCategoryId = null
) : IRequest<ServiceResponse<Guid>>;
