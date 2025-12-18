using Application.Commands.Category.UpdateCategory;
using FluentAssertions;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.IntegrationTests.Commands.Category;

public class UpdateCategoryCommandHandlerIntegrationTests : TestBase
{
	[Fact]
	public async Task Handle_ShouldUpdateCategoryInDatabase()
	{
		// Arrange
		var existing = Domain.Entities.Category.Create("Old name", "old desc");
		DbContext.Categories.Add(existing);
		await DbContext.SaveChangesAsync();

		var repo = new CategoryRepository(DbContext);
		var uow = new UnitOfWork(DbContext);
		var cache = new MemoryCache(new MemoryCacheOptions());
		var handler = new UpdateCategoryCommandHandler(repo, uow, cache, NullLogger<UpdateCategoryCommandHandler>.Instance);

		var command = new UpdateCategoryCommand(existing.Id, "New name", "new desc", null);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();

		var updated = await DbContext.Categories.FirstOrDefaultAsync(c => c.Id == existing.Id);
		updated.Should().NotBeNull();
		updated!.Name.Should().Be("New name");
		updated.Description.Should().Be("new desc");
	}

	[Fact]
	public async Task Handle_WhenSlugAlreadyUsedByOtherCategory_ShouldReturnFailure()
	{
		// Arrange
		var cat1 = Domain.Entities.Category.Create("Cat One");
		var cat2 = Domain.Entities.Category.Create("Cat Two");
		DbContext.Categories.AddRange(cat1, cat2);
		await DbContext.SaveChangesAsync();

		var repo = new CategoryRepository(DbContext);
		var uow = new UnitOfWork(DbContext);
		var cache = new MemoryCache(new MemoryCacheOptions());
		var handler = new UpdateCategoryCommandHandler(repo, uow, cache, NullLogger<UpdateCategoryCommandHandler>.Instance);

		// Act: rename cat2 to cat1 name => slug collision
		var result = await handler.Handle(new UpdateCategoryCommand(cat2.Id, "Cat One"), CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Message.Should().Be("Category with same slug already exists");
	}
}
