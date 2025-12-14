using Domain.Entities;
using Domain.Interfaces.Repositories;
using FluentAssertions;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntegrationTests.Repositories;

/// <summary>
/// Інтеграційні тести для MediaImageRepository
/// </summary>
public class MediaImageRepositoryTests : TestBase
{
	private readonly IMediaImageRepository _repository;

	public MediaImageRepositoryTests()
	{
		_repository = new MediaImageRepository(DbContext);
	}

	[Fact]
	public async Task Add_ShouldAddMediaImageToDatabase()
	{
		// Arrange
		var mediaImage = new MediaImage("test-key.webp", "image/webp", 256, 256, "Test Image");

		// Act
		_repository.Add(mediaImage);
		await DbContext.SaveChangesAsync();

		// Assert
		mediaImage.Should().NotBeNull();
		mediaImage.Id.Should().NotBeEmpty();
		mediaImage.StorageKey.Should().Be("test-key.webp");

		var fromDb = await DbContext.MediaImages.FindAsync(mediaImage.Id);
		fromDb.Should().NotBeNull();
	}

	[Fact]
	public async Task GetByIdAsync_WithValidId_ShouldReturnMediaImage()
	{
		// Arrange
		var mediaImage = new MediaImage("test-key.webp", "image/webp", 256, 256, "Test");
		_repository.Add(mediaImage);
		await DbContext.SaveChangesAsync();

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
		_repository.Add(mediaImage);
		await DbContext.SaveChangesAsync();

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
	public async Task Update_ShouldUpdateMediaImage()
	{
		// Arrange
		var mediaImage = new MediaImage("test-key.webp", "image/webp", 256, 256, "Original Alt");
		_repository.Add(mediaImage);
		await DbContext.SaveChangesAsync();

		// Act
		mediaImage.UpdateMetadata("Updated Alt Text");
		_repository.Update(mediaImage);
		await DbContext.SaveChangesAsync();

		// Assert
		mediaImage.AltText.Should().Be("Updated Alt Text");

		var fromDb = await DbContext.MediaImages.FindAsync(mediaImage.Id);
		fromDb!.AltText.Should().Be("Updated Alt Text");
	}

	[Fact]
	public async Task Delete_ShouldRemoveMediaImageFromDatabase()
	{
		// Arrange
		var mediaImage = new MediaImage("test-key.webp", "image/webp", 256, 256, "Test");
		_repository.Add(mediaImage);
		await DbContext.SaveChangesAsync();
		var id = mediaImage.Id;

		// Act
		_repository.Delete(mediaImage);
		await DbContext.SaveChangesAsync();

		// Assert
		var fromDb = await DbContext.MediaImages.FindAsync(id);
		fromDb.Should().BeNull();
	}

	[Fact]
	public async Task GetAllAsync_ShouldReturnAllMediaImages()
	{
		// Arrange
		_repository.Add(new MediaImage("key1.webp", "image/webp", 256, 256, "Image 1"));
		_repository.Add(new MediaImage("key2.webp", "image/webp", 512, 512, "Image 2"));
		_repository.Add(new MediaImage("key3.webp", "image/webp", 128, 128, "Image 3"));
		await DbContext.SaveChangesAsync();

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
		var product = new Product("Product for gallery");
		var image1 = new MediaImage("product1.webp", "image/webp", 256, 256, "Product Image 1");
		var image2 = new MediaImage("product2.webp", "image/webp", 256, 256, "Product Image 2");
		var image3 = new MediaImage("other.webp", "image/webp", 256, 256, "Other Image");

		DbContext.Products.Add(product);
		_repository.Add(image1);
		_repository.Add(image2);
		_repository.Add(image3);
		product.AddGalleryItem(image1);
		product.AddGalleryItem(image2);
		await DbContext.SaveChangesAsync();

		// Act
		var result = await _repository.GetByProductIdAsync(product.Id);

		// Assert
		result.Should().HaveCount(2);
		result.Select(m => m.StorageKey).Should().Contain(new[] { "product1.webp", "product2.webp" });
	}

	[Fact]
	public async Task GetOrphanedImagesAsync_ShouldReturnOnlyOrphanedImages()
	{
		// Arrange
		var product = new Product("Product with gallery");

		// Create images
		var orphaned1 = new MediaImage("orphaned1.webp", "image/webp", 256, 256, "Orphaned 1");
		var orphaned2 = new MediaImage("orphaned2.webp", "image/webp", 256, 256, "Orphaned 2");
		var productImage = new MediaImage("product.webp", "image/webp", 256, 256, "Product Image");
		var avatarImage = new MediaImage("avatar.webp", "image/webp", 256, 256, "Avatar Image");

		DbContext.Products.Add(product);
		_repository.Add(orphaned1);
		_repository.Add(orphaned2);
		_repository.Add(productImage);
		_repository.Add(avatarImage);
		product.AddGalleryItem(productImage);

		// Create user with avatar
		var user = new User(Guid.NewGuid(), "Test", "User");
		user.SetAvatar(avatarImage);
		DbContext.DomainUsers.Add(user);
		await DbContext.SaveChangesAsync();

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
		_repository.Add(avatarImage);
		await DbContext.SaveChangesAsync();

		var user = new User(Guid.NewGuid(), "Test", "User");
		user.SetAvatar(avatarImage);
		DbContext.DomainUsers.Add(user);
		await DbContext.SaveChangesAsync();

		// Act
		var result = await _repository.GetOrphanedImagesAsync();

		// Assert
		result.Should().BeEmpty();
	}
}
