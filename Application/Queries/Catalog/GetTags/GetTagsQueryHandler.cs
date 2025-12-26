using Application.DTOs;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Catalog.GetTags;

public sealed class GetTagsQueryHandler : IRequestHandler<GetTagsQuery, ServiceResponse<IReadOnlyList<TagDto>>>
{
	private readonly ITagRepository _tagRepository;
	private readonly ILogger<GetTagsQueryHandler> _logger;

	public GetTagsQueryHandler(ITagRepository tagRepository, ILogger<GetTagsQueryHandler> logger)
	{
		_tagRepository = tagRepository;
		_logger = logger;
	}

	public async Task<ServiceResponse<IReadOnlyList<TagDto>>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
	{
		try
		{
			var tags = await _tagRepository.GetAllAsync();
			var payload = tags
				.Select(t => new TagDto(t.Id, t.Name, t.Slug, t.Description))
				.ToList()
				.AsReadOnly();

			return new ServiceResponse<IReadOnlyList<TagDto>>(true, "Tags retrieved successfully", payload);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving tags");
			return new ServiceResponse<IReadOnlyList<TagDto>>(false, $"Error: {ex.Message}");
		}
	}
}
