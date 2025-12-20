using System.Security.Claims;
using Application.Commands.Product.CreateProduct;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
	private readonly IMediator _mediator;

	public ProductsController(IMediator mediator)
	{
		_mediator = mediator;
	}

	public sealed record CreateProductRequest(
		string Name,
		string? Description,
		List<Guid>? CategoryIds,
		decimal Price,
		int StockQuantity,
		Dictionary<string, object?>? Attributes = null,
		List<Guid>? TagIds = null
	);

	/// <summary>
	/// Створити продукт (для поточного продавця)
	/// </summary>
	[HttpPost]
	[Authorize(Policy = "Permission:products.create")]
	public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
	{
		var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (string.IsNullOrWhiteSpace(idClaim)) return Unauthorized();
		if (!Guid.TryParse(idClaim, out var userId)) return Unauthorized();

		var command = new CreateProductCommand(
			userId,
			request.Name,
			request.Description,
			request.CategoryIds ?? new List<Guid>(),
			request.Price,
			request.StockQuantity,
			request.Attributes,
			request.TagIds
		);

		var result = await _mediator.Send(command);
		if (!result.IsSuccess) return BadRequest(result);
		return Ok(result);
	}
}
