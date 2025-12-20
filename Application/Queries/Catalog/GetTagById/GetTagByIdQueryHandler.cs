using Application.DTOs;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Catalog.GetTagById;

public sealed class GetTagByIdQueryHandler : IRequestHandler<GetTagByIdQuery, ServiceResponse<TagDto>>
{
	private readonly ITagRepository _tagRepository;
	private readonly ILogger<GetTagByIdQueryHandler> _logger;

	public GetTagByIdQueryHandler(ITagRepository tagRepository, ILogger<GetTagByIdQueryHandler> logger)
	{
		_tagRepository = tagRepository;
		_logger = logger;
	}

	public async Task<ServiceResponse<TagDto>> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
	{
		try
		{
			var tag = await _tagRepository.GetByIdAsync(request.Id);
			if (tag is null)
			{
				return new ServiceResponse<TagDto>(false, "Tag not found");
			}

			return new ServiceResponse<TagDto>(true, "Tag retrieved successfully",
				new TagDto(tag.Id, tag.Name, tag.Slug, tag.Description));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error retrieving tag {TagId}", request.Id);
			return new ServiceResponse<TagDto>(false, $"Error: {ex.Message}");
		}
	}
}
