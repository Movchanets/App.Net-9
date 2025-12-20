using Application.Commands.Category.CreateCategory;
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
