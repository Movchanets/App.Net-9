using Application.Commands.Category.DeleteCategory;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Commands.Category;

public class DeleteCategoryCommandHandlerTests
{
	private readonly Mock<ICategoryRepository> _categoryRepository = new();
	private readonly Mock<IUnitOfWork> _unitOfWork = new();
	private readonly Mock<IMemoryCache> _cache = new();
	private readonly Mock<ILogger<DeleteCategoryCommandHandler>> _logger = new();

	private DeleteCategoryCommandHandler CreateSut()
		=> new(_categoryRepository.Object, _unitOfWork.Object, _cache.Object, _logger.Object);

	[Fact]
	public async Task Handle_WhenCategoryNotFound_ReturnsFailure()
	{
		// Arrange
		var id = Guid.NewGuid();
		_categoryRepository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((Domain.Entities.Category?)null);
		var sut = CreateSut();

		// Act
		var res = await sut.Handle(new DeleteCategoryCommand(id), CancellationToken.None);

		// Assert
		res.IsSuccess.Should().BeFalse();
		res.Message.Should().Be("Category not found");
		_unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WhenHasSubcategories_ReturnsFailure()
	{
		// Arrange
		var category = Domain.Entities.Category.Create("Parent");
		var child = Domain.Entities.Category.Create("Child");

		_categoryRepository.Setup(x => x.GetByIdAsync(category.Id)).ReturnsAsync(category);
		_categoryRepository.Setup(x => x.GetSubCategoriesAsync(category.Id)).ReturnsAsync(new[] { child });

		var sut = CreateSut();

		// Act
		var res = await sut.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

		// Assert
		res.IsSuccess.Should().BeFalse();
		res.Message.Should().Be("Category has subcategories");
		_categoryRepository.Verify(x => x.Delete(It.IsAny<Domain.Entities.Category>()), Times.Never);
		_unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WhenValid_DeletesAndInvalidatesCache()
	{
		// Arrange
		var category = Domain.Entities.Category.Create("Cat");
		_categoryRepository.Setup(x => x.GetByIdAsync(category.Id)).ReturnsAsync(category);
		_categoryRepository.Setup(x => x.GetSubCategoriesAsync(category.Id)).ReturnsAsync(Array.Empty<Domain.Entities.Category>());
		_unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

		var sut = CreateSut();

		// Act
		var res = await sut.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

		// Assert
		res.IsSuccess.Should().BeTrue();
		_categoryRepository.Verify(x => x.Delete(category), Times.Once);
		_unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
		_cache.Verify(x => x.Remove("categories:all"), Times.Once);
		_cache.Verify(x => x.Remove("categories:top-level"), Times.Once);
	}
}
