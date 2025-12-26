using System.Security.Claims;
using Application.Commands.Product.CreateProduct;
using Application.Queries.Catalog.GetProductById;
using Application.Queries.Catalog.GetProductBySkuCode;
using Application.Queries.Catalog.GetProducts;
using Application.Queries.Catalog.GetProductsByCategoryId;
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

	/// <summary>
	/// Отримати список продуктів
	/// </summary>
	[HttpGet]
	[AllowAnonymous]
	public async Task<IActionResult> GetAll()
	{
		var result = await _mediator.Send(new GetProductsQuery());
		if (!result.IsSuccess) return BadRequest(result);
		return Ok(result);
	}

	/// <summary>
	/// Отримати продукт по Id
	/// </summary>
	[HttpGet("{id:guid}")]
	[AllowAnonymous]
	public async Task<IActionResult> GetById([FromRoute] Guid id)
	{
		var result = await _mediator.Send(new GetProductByIdQuery(id));
		if (!result.IsSuccess) return NotFound(result);
		return Ok(result);
	}

	/// <summary>
	/// Отримати продукт по skuCode
	/// </summary>
	[HttpGet("by-sku/{skuCode}")]
	[AllowAnonymous]
	public async Task<IActionResult> GetBySkuCode([FromRoute] string skuCode)
	{
		var result = await _mediator.Send(new GetProductBySkuCodeQuery(skuCode));
		if (!result.IsSuccess) return NotFound(result);
		return Ok(result);
	}

	/// <summary>
	/// Отримати продукти по categoryId
	/// </summary>
	[HttpGet("by-category/{categoryId:guid}")]
	[AllowAnonymous]
	public async Task<IActionResult> GetByCategory([FromRoute] Guid categoryId)
	{
		var result = await _mediator.Send(new GetProductsByCategoryIdQuery(categoryId));
		if (!result.IsSuccess) return BadRequest(result);
		return Ok(result);
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
