using Application.Commands.Product.UpdateProduct;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Commands.Product;

public class UpdateProductCommandHandlerTests
{
	private readonly Mock<IProductRepository> _productRepository = new();
	private readonly Mock<IStoreRepository> _storeRepository = new();
	private readonly Mock<ICategoryRepository> _categoryRepository = new();
	private readonly Mock<ITagRepository> _tagRepository = new();
	private readonly Mock<IUnitOfWork> _unitOfWork = new();
	private readonly Mock<ILogger<UpdateProductCommandHandler>> _logger = new();

	private UpdateProductCommandHandler CreateSut()
		=> new(
			_productRepository.Object,
			_storeRepository.Object,
			_categoryRepository.Object,
			_tagRepository.Object,
			_unitOfWork.Object,
			_logger.Object);

	[Fact]
	public async Task Handle_WhenProductNotFound_ReturnsFailure()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var store = Domain.Entities.Store.Create(userId, "My Store", null);
		_storeRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(store);
		_productRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.Product?)null);

		var sut = CreateSut();
		var cmd = new UpdateProductCommand(userId, Guid.NewGuid(), "New", null, Guid.NewGuid());

		// Act
		var res = await sut.Handle(cmd, CancellationToken.None);

		// Assert
		res.IsSuccess.Should().BeFalse();
		res.Message.Should().Be("Product not found");
		_productRepository.Verify(x => x.Update(It.IsAny<Domain.Entities.Product>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WhenCategoryNotFound_ReturnsFailure()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var store = Domain.Entities.Store.Create(userId, "My Store", null);
		var product = new Domain.Entities.Product("Old", null);
		store.AddProduct(product);

		_storeRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(store);
		_productRepository.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);
		_categoryRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Domain.Entities.Category?)null);

		var sut = CreateSut();
		var cmd = new UpdateProductCommand(userId, product.Id, "New", null, Guid.NewGuid());

		// Act
		var res = await sut.Handle(cmd, CancellationToken.None);

		// Assert
		res.IsSuccess.Should().BeFalse();
		res.Message.Should().Be("Category not found");
		_productRepository.Verify(x => x.Update(It.IsAny<Domain.Entities.Product>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WhenValidRequest_UpdatesProduct()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var store = Domain.Entities.Store.Create(userId, "My Store", null);
		var product = new Domain.Entities.Product("Old", "old");
		store.AddProduct(product);
		var category = Domain.Entities.Category.Create("Electronics");
		var tag = Domain.Entities.Tag.Create("Hot");

		_storeRepository.Setup(x => x.GetByUserIdAsync(userId)).ReturnsAsync(store);
		_productRepository.Setup(x => x.GetByIdAsync(product.Id)).ReturnsAsync(product);
		_categoryRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(category);
		_tagRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(tag);
		_unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

		var sut = CreateSut();
		var cmd = new UpdateProductCommand(userId, product.Id, "New", "new desc", Guid.NewGuid(), new List<Guid> { Guid.NewGuid() });

		// Act
		var res = await sut.Handle(cmd, CancellationToken.None);

		// Assert
		res.IsSuccess.Should().BeTrue();
		res.Message.Should().Be("Product updated successfully");
		product.Name.Should().Be("New");
		product.Description.Should().Be("new desc");
		_productRepository.Verify(x => x.Update(product), Times.Once);
		_unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}
