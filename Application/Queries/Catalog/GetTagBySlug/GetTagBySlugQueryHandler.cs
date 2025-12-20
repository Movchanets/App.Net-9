using Application.DTOs;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Catalog.GetTagBySlug;

public sealed class GetTagBySlugQueryHandler : IRequestHandler<GetTagBySlugQuery, ServiceResponse<TagDto>>
{
	private readonly ITagRepository _tagRepository;
	private readonly ILogger<GetTagBySlugQueryHandler> _logger;

	public GetTagBySlugQueryHandler(ITagRepository tagRepository, ILogger<GetTagBySlugQueryHandler> logger)
	{
		_tagRepository = tagRepository;
		_logger = logger;
	}

	public async Task<ServiceResponse<TagDto>> Handle(GetTagBySlugQuery request, CancellationToken cancellationToken)
	{
		try
		{
			var slug = request.Slug?.Trim();
			if (string.IsNullOrWhiteSpace(slug))
			{
				return new ServiceResponse<TagDto>(false, "Slug is required");
			}

			var tag = await _tagRepository.GetBySlugAsync(slug);
			if (tag is null)
			{
				return new ServiceResponse<TagDto>(false, "Tag not found");
			}

			return new ServiceResponse<TagDto>(true, "Tag retrieved successfully",
				new TagDto(tag.Id, tag.Name, tag.Slug, tag.Description));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving tag by slug {Slug}", request.Slug);
			return new ServiceResponse<TagDto>(false, $"Error: {ex.Message}");
		}
	}
}
