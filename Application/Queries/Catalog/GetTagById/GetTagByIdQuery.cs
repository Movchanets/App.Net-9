using Application.DTOs;
using MediatR;

namespace Application.Queries.Catalog.GetTagById;

public sealed record GetTagByIdQuery(Guid Id) : IRequest<ServiceResponse<TagDto>>;
