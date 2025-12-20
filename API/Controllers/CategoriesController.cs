using Application.Commands.Category.CreateCategory;
using Application.Queries.Catalog.GetCategories;
using Application.Queries.Catalog.GetCategoryById;
using Application.Queries.Catalog.GetCategoryBySlug;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController : ControllerBase
{
	private readonly IMediator _mediator;

	public CategoriesController(IMediator mediator)
	{
		_mediator = mediator;
	}

	/// <summary>
	/// Отримати список категорій
	/// </summary>
	[HttpGet]
	[AllowAnonymous]
	public async Task<IActionResult> GetAll([FromQuery] Guid? parentCategoryId = null, [FromQuery] bool topLevelOnly = false)
	{
		var result = await _mediator.Send(new GetCategoriesQuery(parentCategoryId, topLevelOnly));
		if (!result.IsSuccess) return BadRequest(result);
		return Ok(result);
	}

	/// <summary>
	/// Отримати категорію по Id
	/// </summary>
	[HttpGet("{id:guid}")]
	[AllowAnonymous]
	public async Task<IActionResult> GetById([FromRoute] Guid id)
	{
		var result = await _mediator.Send(new GetCategoryByIdQuery(id));
		if (!result.IsSuccess) return NotFound(result);
		return Ok(result);
	}

	/// <summary>
	/// Отримати категорію по slug
	/// </summary>
	[HttpGet("slug/{slug}")]
	[AllowAnonymous]
	public async Task<IActionResult> GetBySlug([FromRoute] string slug)
	{
		var result = await _mediator.Send(new GetCategoryBySlugQuery(slug));
		if (!result.IsSuccess) return NotFound(result);
		return Ok(result);
	}

	/// <summary>
	/// Створити категорію
	/// </summary>
	[HttpPost]
	[Authorize(Policy = "Permission:categories.manage")]
	public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
	{
		var result = await _mediator.Send(command);
		if (!result.IsSuccess) return BadRequest(result);
		return Ok(result);
	}
}
