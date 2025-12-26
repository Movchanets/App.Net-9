using Application.DTOs;
using MediatR;

namespace Application.Queries.Catalog.GetTagBySlug;

public sealed record GetTagBySlugQuery(string Slug) : IRequest<ServiceResponse<TagDto>>;
