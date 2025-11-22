using Application.Interfaces;
using FluentAssertions;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Repositories;

/// <summary>
/// Інтеграційні тести для UnitOfWork
/// </summary>
public class UnitOfWorkTests : IDisposable
{
	private readonly AppDbContext _dbContext;
	private readonly IUnitOfWork _unitOfWork;

	public UnitOfWorkTests()
	{
		// Setup in-memory database
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
			.Options;

		_dbContext = new AppDbContext(options);
		_dbContext.Database.EnsureCreated();

		_unitOfWork = new UnitOfWork(_dbContext);
	}

	public void Dispose()
	{
		_dbContext?.Database.EnsureDeleted();
		_dbContext?.Dispose();
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
		_dbContext.MediaImages.Add(mediaImage);

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

		_dbContext.MediaImages.AddRange(image1, image2, image3);

		// Act
		var result = await _unitOfWork.SaveChangesAsync();

		// Assert
		result.Should().Be(3);
		image1.Id.Should().NotBeEmpty();
		image2.Id.Should().NotBeEmpty();
		image3.Id.Should().NotBeEmpty();
	}

	[Fact]
	public async Task BeginTransactionAsync_WithInMemoryDatabase_ShouldReturnNoOpTransaction()
	{
		// Act
		var transaction = await _unitOfWork.BeginTransactionAsync();

		// Assert
		transaction.Should().NotBeNull();
		transaction.TransactionId.Should().NotBeEmpty();

		// NoOpTransaction should not throw on commit/rollback
		await transaction.CommitAsync();
		await transaction.RollbackAsync();
	}

	[Fact]
	public async Task BeginTransactionAsync_NoOpTransaction_ShouldHandleMultipleOperations()
	{
		// Arrange
		var transaction = await _unitOfWork.BeginTransactionAsync();

		// Act & Assert - multiple operations should not throw
		await transaction.CommitAsync();
		await transaction.RollbackAsync(); // Should be safe to call even after commit
		await transaction.CommitAsync();   // Should be safe to call multiple times
	}

	[Fact]
	public async Task UnitOfWork_ShouldMaintainConsistency_AcrossMultipleOperations()
	{
		// Arrange
		var image1 = new Domain.Entities.MediaImage("key1.webp", "image/webp", 256, 256, "Image 1");
		var image2 = new Domain.Entities.MediaImage("key2.webp", "image/webp", 256, 256, "Image 2");

		// Act
		_dbContext.MediaImages.Add(image1);
		var count1 = await _unitOfWork.SaveChangesAsync();

		_dbContext.MediaImages.Add(image2);
		var count2 = await _unitOfWork.SaveChangesAsync();

		// Assert
		count1.Should().Be(1);
		count2.Should().Be(1);

		var allImages = await _dbContext.MediaImages.ToListAsync();
		allImages.Should().HaveCount(2);
		allImages.Select(i => i.StorageKey).Should().Contain(new[] { "key1.webp", "key2.webp" });
	}
}