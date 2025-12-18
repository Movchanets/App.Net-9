using Application.Commands.Category.DeleteCategory;
using FluentAssertions;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.IntegrationTests.Commands.Category;

public class DeleteCategoryCommandHandlerIntegrationTests : TestBase
{
	[Fact]
	public async Task Handle_WhenHasSubcategories_ShouldReturnFailure()
	{
		// Arrange
		var parent = Domain.Entities.Category.Create("Parent");
		var child = Domain.Entities.Category.Create("Child");
		child.SetParent(parent);

		DbContext.Categories.AddRange(parent, child);
		await DbContext.SaveChangesAsync();

		var repo = new CategoryRepository(DbContext);
		var uow = new UnitOfWork(DbContext);
		var cache = new MemoryCache(new MemoryCacheOptions());
		var handler = new DeleteCategoryCommandHandler(repo, uow, cache, NullLogger<DeleteCategoryCommandHandler>.Instance);

		// Act
		var result = await handler.Handle(new DeleteCategoryCommand(parent.Id), CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Message.Should().Be("Category has subcategories");
		(await DbContext.Categories.CountAsync()).Should().Be(2);
	}

	[Fact]
	public async Task Handle_WhenLeafCategory_ShouldDeleteFromDatabase()
	{
		// Arrange
		var category = Domain.Entities.Category.Create("Leaf");
		DbContext.Categories.Add(category);
		await DbContext.SaveChangesAsync();

		var repo = new CategoryRepository(DbContext);
		var uow = new UnitOfWork(DbContext);
		var cache = new MemoryCache(new MemoryCacheOptions());
		var handler = new DeleteCategoryCommandHandler(repo, uow, cache, NullLogger<DeleteCategoryCommandHandler>.Instance);

		// Act
		var result = await handler.Handle(new DeleteCategoryCommand(category.Id), CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		(await DbContext.Categories.CountAsync()).Should().Be(0);
	}
}
