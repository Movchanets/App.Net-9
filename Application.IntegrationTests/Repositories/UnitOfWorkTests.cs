using Application.Interfaces;
using FluentAssertions;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Repositories;

/// <summary>
/// Інтеграційні тести для UnitOfWork
/// </summary>
public class UnitOfWorkTests : TestBase
{
	private readonly IUnitOfWork _unitOfWork;

	public UnitOfWorkTests()
	{
		_unitOfWork = new UnitOfWork(DbContext);
	}

	[Fact]
	public async Task SaveChangesAsync_WhenNoChanges_ShouldReturnZero()
	{
		// Act
		var result = await _unitOfWork.SaveChangesAsync();

		// Assert
		result.Should().Be(0);
	}

	[Fact]
	public async Task SaveChangesAsync_WhenEntityAdded_ShouldReturnOne()
	{
		// Arrange
		var mediaImage = new Domain.Entities.MediaImage("test-key.webp", "image/webp", 256, 256, "Test Image");
		DbContext.MediaImages.Add(mediaImage);

		// Act
		var result = await _unitOfWork.SaveChangesAsync();

		// Assert
		result.Should().Be(1);
		mediaImage.Id.Should().NotBeEmpty();
	}

	[Fact]
	public async Task SaveChangesAsync_WhenMultipleEntitiesAdded_ShouldReturnCorrectCount()
	{
		// Arrange
		var image1 = new Domain.Entities.MediaImage("key1.webp", "image/webp", 256, 256, "Image 1");
		var image2 = new Domain.Entities.MediaImage("key2.webp", "image/webp", 256, 256, "Image 2");
		var image3 = new Domain.Entities.MediaImage("key3.webp", "image/webp", 256, 256, "Image 3");

		DbContext.MediaImages.AddRange(image1, image2, image3);

		// Act
		var result = await _unitOfWork.SaveChangesAsync();

		// Assert
		result.Should().Be(3);
		image1.Id.Should().NotBeEmpty();
		image2.Id.Should().NotBeEmpty();
		image3.Id.Should().NotBeEmpty();
	}

	[Fact]
	public async Task BeginTransactionAsync_ShouldRollbackChanges()
	{
		var initialCount = await DbContext.MediaImages.CountAsync();

		await using var transaction = await _unitOfWork.BeginTransactionAsync();
		DbContext.MediaImages.Add(new Domain.Entities.MediaImage("tx-key.webp", "image/webp", 64, 64, "Tx"));
		await DbContext.SaveChangesAsync();

		await transaction.RollbackAsync();

		var finalCount = await DbContext.MediaImages.CountAsync();
		finalCount.Should().Be(initialCount);
	}

	[Fact]
	public async Task BeginTransactionAsync_ShouldCommitChanges()
	{
		await using var transaction = await _unitOfWork.BeginTransactionAsync();
		var image = new Domain.Entities.MediaImage("tx-commit.webp", "image/webp", 64, 64, "Tx Commit");

		DbContext.MediaImages.Add(image);
		await DbContext.SaveChangesAsync();
		await transaction.CommitAsync();

		var exists = await DbContext.MediaImages.AnyAsync(m => m.Id == image.Id);
		exists.Should().BeTrue();
	}

	[Fact]
	public async Task UnitOfWork_ShouldMaintainConsistency_AcrossMultipleOperations()
	{
		// Arrange
		var image1 = new Domain.Entities.MediaImage("key1.webp", "image/webp", 256, 256, "Image 1");
		var image2 = new Domain.Entities.MediaImage("key2.webp", "image/webp", 256, 256, "Image 2");

		// Act
		DbContext.MediaImages.Add(image1);
		var count1 = await _unitOfWork.SaveChangesAsync();

		DbContext.MediaImages.Add(image2);
		var count2 = await _unitOfWork.SaveChangesAsync();

		// Assert
		count1.Should().Be(1);
		count2.Should().Be(1);

		var allImages = await DbContext.MediaImages.ToListAsync();
		allImages.Should().HaveCount(2);
		allImages.Select(i => i.StorageKey).Should().Contain(new[] { "key1.webp", "key2.webp" });
	}
}