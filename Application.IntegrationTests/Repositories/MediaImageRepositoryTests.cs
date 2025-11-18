using Domain.Entities;
using Domain.Interfaces.Repositories;
using FluentAssertions;
using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Repositories;

/// <summary>
/// Інтеграційні тести для MediaImageRepository
/// </summary>
public class MediaImageRepositoryTests : IDisposable
{
	private readonly AppDbContext _dbContext;
	private readonly IMediaImageRepository _repository;

	public MediaImageRepositoryTests()
	{
		// Setup in-memory database
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
			.Options;

		_dbContext = new AppDbContext(options);
		_dbContext.Database.EnsureCreated();

		_repository = new MediaImageRepository(_dbContext);
	}

	public void Dispose()
	{
		_dbContext?.Database.EnsureDeleted();
		_dbContext?.Dispose();
	}

	[Fact]
	public async Task AddAsync_ShouldAddMediaImageToDatabase()
	{
		// Arrange
		var mediaImage = new MediaImage("test-key.webp", "image/webp", 256, 256, "Test Image");

		// Act
		var result = await _repository.AddAsync(mediaImage);

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().NotBeEmpty();
		result.StorageKey.Should().Be("test-key.webp");

		var fromDb = await _dbContext.MediaImages.FindAsync(result.Id);
		fromDb.Should().NotBeNull();
	}

	[Fact]
	public async Task GetByIdAsync_WithValidId_ShouldReturnMediaImage()
	{
		// Arrange
		var mediaImage = new MediaImage("test-key.webp", "image/webp", 256, 256, "Test");
		await _repository.AddAsync(mediaImage);

		// Act
		var result = await _repository.GetByIdAsync(mediaImage.Id);

		// Assert
		result.Should().NotBeNull();
		result!.Id.Should().Be(mediaImage.Id);
		result.StorageKey.Should().Be("test-key.webp");
	}

	[Fact]
	public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
	{
		// Arrange
		var nonExistentId = Guid.NewGuid();

		// Act
		var result = await _repository.GetByIdAsync(nonExistentId);

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task GetByStorageKeyAsync_WithValidKey_ShouldReturnMediaImage()
	{
		// Arrange
		var mediaImage = new MediaImage("unique-key.webp", "image/webp", 512, 512, "Test");
		await _repository.AddAsync(mediaImage);

		// Act
		var result = await _repository.GetByStorageKeyAsync("unique-key.webp");

		// Assert
		result.Should().NotBeNull();
		result!.StorageKey.Should().Be("unique-key.webp");
		result.Width.Should().Be(512);
	}

	[Fact]
	public async Task GetByStorageKeyAsync_WithInvalidKey_ShouldReturnNull()
	{
		// Act
		var result = await _repository.GetByStorageKeyAsync("non-existent-key.webp");

		// Assert
		result.Should().BeNull();
	}

	[Fact]
	public async Task UpdateAsync_ShouldUpdateMediaImage()
	{
		// Arrange
		var mediaImage = new MediaImage("test-key.webp", "image/webp", 256, 256, "Original Alt");
		await _repository.AddAsync(mediaImage);

		// Act
		mediaImage.UpdateMetadata("Updated Alt Text");
		var result = await _repository.UpdateAsync(mediaImage);

		// Assert
		result.AltText.Should().Be("Updated Alt Text");

		var fromDb = await _dbContext.MediaImages.FindAsync(mediaImage.Id);
		fromDb!.AltText.Should().Be("Updated Alt Text");
	}

	[Fact]
	public async Task DeleteAsync_ShouldRemoveMediaImageFromDatabase()
	{
		// Arrange
		var mediaImage = new MediaImage("test-key.webp", "image/webp", 256, 256, "Test");
		await _repository.AddAsync(mediaImage);
		var id = mediaImage.Id;

		// Act
		await _repository.DeleteAsync(mediaImage);

		// Assert
		var fromDb = await _dbContext.MediaImages.FindAsync(id);
		fromDb.Should().BeNull();
	}

	[Fact]
	public async Task GetAllAsync_ShouldReturnAllMediaImages()
	{
		// Arrange
		await _repository.AddAsync(new MediaImage("key1.webp", "image/webp", 256, 256, "Image 1"));
		await _repository.AddAsync(new MediaImage("key2.webp", "image/webp", 512, 512, "Image 2"));
		await _repository.AddAsync(new MediaImage("key3.webp", "image/webp", 128, 128, "Image 3"));

		// Act
		var result = await _repository.GetAllAsync();

		// Assert
		result.Should().HaveCount(3);
		result.Select(m => m.StorageKey).Should().Contain(new[] { "key1.webp", "key2.webp", "key3.webp" });
	}

	[Fact]
	public async Task GetByProductIdAsync_ShouldReturnOnlyProductImages()
	{
		// Arrange
		var productId = Guid.NewGuid();

		var image1 = new MediaImage("product1.webp", "image/webp", 256, 256, "Product Image 1");
		var image2 = new MediaImage("product2.webp", "image/webp", 256, 256, "Product Image 2");
		var image3 = new MediaImage("other.webp", "image/webp", 256, 256, "Other Image");

		await _repository.AddAsync(image1);
		await _repository.AddAsync(image2);
		await _repository.AddAsync(image3);

		// Manually set ProductId (since we don't have Product entity in this test)
		image1.GetType().GetProperty("ProductId")!.SetValue(image1, productId);
		image2.GetType().GetProperty("ProductId")!.SetValue(image2, productId);
		await _dbContext.SaveChangesAsync();

		// Act
		var result = await _repository.GetByProductIdAsync(productId);

		// Assert
		result.Should().HaveCount(2);
		result.Select(m => m.StorageKey).Should().Contain(new[] { "product1.webp", "product2.webp" });
	}

	[Fact]
	public async Task GetOrphanedImagesAsync_ShouldReturnOnlyOrphanedImages()
	{
		// Arrange
		var productId = Guid.NewGuid();

		// Create images
		var orphaned1 = new MediaImage("orphaned1.webp", "image/webp", 256, 256, "Orphaned 1");
		var orphaned2 = new MediaImage("orphaned2.webp", "image/webp", 256, 256, "Orphaned 2");
		var productImage = new MediaImage("product.webp", "image/webp", 256, 256, "Product Image");
		var avatarImage = new MediaImage("avatar.webp", "image/webp", 256, 256, "Avatar Image");

		await _repository.AddAsync(orphaned1);
		await _repository.AddAsync(orphaned2);
		await _repository.AddAsync(productImage);
		await _repository.AddAsync(avatarImage);

		// Set ProductId for product image
		productImage.GetType().GetProperty("ProductId")!.SetValue(productImage, productId);
		await _dbContext.SaveChangesAsync();

		// Create user with avatar
		var user = new User(Guid.NewGuid(), "Test", "User");
		user.SetAvatar(avatarImage);
		_dbContext.DomainUsers.Add(user);
		await _dbContext.SaveChangesAsync();

		// Act
		var result = await _repository.GetOrphanedImagesAsync();

		// Assert
		result.Should().HaveCount(2);
		result.Select(m => m.StorageKey).Should().Contain(new[] { "orphaned1.webp", "orphaned2.webp" });
		result.Select(m => m.StorageKey).Should().NotContain(new[] { "product.webp", "avatar.webp" });
	}

	[Fact]
	public async Task GetOrphanedImagesAsync_WhenNoOrphanedImages_ShouldReturnEmpty()
	{
		// Arrange
		var avatarImage = new MediaImage("avatar.webp", "image/webp", 256, 256, "Avatar");
		await _repository.AddAsync(avatarImage);

		var user = new User(Guid.NewGuid(), "Test", "User");
		user.SetAvatar(avatarImage);
		_dbContext.DomainUsers.Add(user);
		await _dbContext.SaveChangesAsync();

		// Act
		var result = await _repository.GetOrphanedImagesAsync();

		// Assert
		result.Should().BeEmpty();
	}
}
