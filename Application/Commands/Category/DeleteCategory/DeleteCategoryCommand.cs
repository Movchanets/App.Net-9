using Application.DTOs;
using MediatR;

namespace Application.Commands.Category.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest<ServiceResponse>;
