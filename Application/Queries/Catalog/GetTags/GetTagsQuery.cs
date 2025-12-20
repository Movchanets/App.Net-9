using Application.DTOs;
using MediatR;

namespace Application.Queries.Catalog.GetTags;

public sealed record GetTagsQuery() : IRequest<ServiceResponse<IReadOnlyList<TagDto>>>;
