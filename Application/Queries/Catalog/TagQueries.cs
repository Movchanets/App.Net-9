using Application.DTOs;
using Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Queries.Catalog;

public sealed record GetTagsQuery() : IRequest<ServiceResponse<IReadOnlyList<TagDto>>>;

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

public sealed record GetTagByIdQuery(Guid Id) : IRequest<ServiceResponse<TagDto>>;

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

public sealed record GetTagBySlugQuery(string Slug) : IRequest<ServiceResponse<TagDto>>;

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
