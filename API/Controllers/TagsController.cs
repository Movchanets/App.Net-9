using Application.Commands.Tag.CreateTag;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class TagsController : ControllerBase
{
	private readonly IMediator _mediator;

	public TagsController(IMediator mediator)
	{
		_mediator = mediator;
	}

	/// <summary>
	/// Створити тег
	/// </summary>
	[HttpPost]
	[Authorize(Policy = "Permission:tags.manage")]
	public async Task<IActionResult> Create([FromBody] CreateTagCommand command)
	{
		var result = await _mediator.Send(command);
		if (!result.IsSuccess) return BadRequest(result);
		return Ok(result);
	}
}
