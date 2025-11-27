using Application.Interfaces;
using Application.Mapping;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces.Repositories;
using FluentAssertions;
using Infrastructure;
using Infrastructure.Entities.Identity;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.IO;

namespace Infrastructure.IntegrationTests.Commands;

/// <summary>
/// Інтеграційні тести для завантаження зображення профілю
/// Перевіряють РЕАЛЬНЕ збереження MediaImage та оновлення User в БД
/// </summary>
public class UploadProfilePictureIntegrationTests : IDisposable
{
	private readonly AppDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly RoleManager<RoleEntity> _roleManager;
	private readonly IUserRepository _userRepository;
	private readonly IMediaImageRepository _mediaImageRepository;
	private readonly Mock<IFileStorage> _fileStorageMock;
	private readonly Mock<IImageService> _imageServiceMock;
	private readonly Application.Interfaces.IUserService _userService;
	private readonly ServiceProvider _serviceProvider;

	public UploadProfilePictureIntegrationTests()
	{
		// Create service collection for DI
		var services = new ServiceCollection();
		services.AddLogging();

		// Setup in-memory database (unique per test)
		var dbName = $"TestDb_{Guid.NewGuid()}";
		services.AddDbContext<AppDbContext>(options =>
			options.UseInMemoryDatabase(dbName));

		// Setup Identity
		services.AddIdentity<ApplicationUser, RoleEntity>(options =>
			{
				options.Password.RequireDigit = true;
				options.Password.RequiredLength = 6;
				options.User.RequireUniqueEmail = true;
			})
			.AddEntityFrameworkStores<AppDbContext>()
			.AddDefaultTokenProviders();

		// Register repositories
		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<IMediaImageRepository, MediaImageRepository>();
		services.AddScoped<Application.Interfaces.IUnitOfWork, Infrastructure.Services.UnitOfWork>();

		// Build service provider
		_serviceProvider = services.BuildServiceProvider();

		// Get services
		_dbContext = _serviceProvider.GetRequiredService<AppDbContext>();
		_userManager = _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		_roleManager = _serviceProvider.GetRequiredService<RoleManager<RoleEntity>>();
		_userRepository = _serviceProvider.GetRequiredService<IUserRepository>();
		_mediaImageRepository = _serviceProvider.GetRequiredService<IMediaImageRepository>();
		var unitOfWork = _serviceProvider.GetRequiredService<Application.Interfaces.IUnitOfWork>();

		// Ensure database is created
		_dbContext.Database.EnsureCreated();

		// Setup mocks
		_fileStorageMock = new Mock<IFileStorage>();
		_imageServiceMock = new Mock<IImageService>();

		// Setup default mock behaviors
		_fileStorageMock
			.Setup(x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync("test-storage-key.webp");

		_fileStorageMock
			.Setup(x => x.GetPublicUrl(It.IsAny<string>()))
			.Returns<string>(key => $"/uploads/{key}");

		var processedStream = new MemoryStream(new byte[] { 0x52, 0x49, 0x46, 0x46 }); // WebP header
		_imageServiceMock
			.Setup(x => x.ProcessAsync(It.IsAny<Stream>(), It.IsAny<ImageResizeMode>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(new ProcessedImageResult(processedStream, "image/webp", ".webp", 256, 256));

		// Create AutoMapper configuration
		var mapperConfig = new MapperConfiguration(mc =>
		{
			mc.AddProfile(new AutoMapperProfile());
		}, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
		var mapper = mapperConfig.CreateMapper();

		// Create UserService with real dependencies and mocked file/image services
		_userService = new Infrastructure.Services.UserService(
			_userManager,
			_roleManager,
			_userRepository,
			_fileStorageMock.Object,
			_imageServiceMock.Object,
			_mediaImageRepository,
			unitOfWork,
			mapper
		);
	}

	public void Dispose()
	{
		_dbContext?.Database.EnsureDeleted();
		_dbContext?.Dispose();
		_serviceProvider?.Dispose();
	}

	[Fact]
	public async Task UpdateProfilePictureAsync_ShouldCreateMediaImageInDatabase()
	{
		// Arrange - create a test user first
		var user = new ApplicationUser
		{
			Email = "testuser@example.com",
			UserName = "testuser"
		};
		await _userManager.CreateAsync(user, "Password123!");

		var domainUser = new User(user.Id, "Test", "User");
		_userRepository.Add(domainUser);
		await _dbContext.SaveChangesAsync();

		var fileStream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }); // JPEG header
		var fileName = "avatar.jpg";
		var contentType = "image/jpeg";

		// Act
		var result = await _userService.UpdateProfilePictureAsync(user.Id, fileStream, fileName, contentType);

		// Assert
		result.Should().NotBeNull();
		result!.AvatarUrl.Should().Be("/uploads/test-storage-key.webp");

		// Verify MediaImage is in database
		var allImages = await _mediaImageRepository.GetAllAsync();
		var mediaImage = allImages.FirstOrDefault();
		mediaImage.Should().NotBeNull();
		mediaImage!.StorageKey.Should().Be("test-storage-key.webp");
		mediaImage.MimeType.Should().Be("image/webp");
		mediaImage.Width.Should().Be(256);
		mediaImage.Height.Should().Be(256);
		mediaImage.AltText.Should().Contain("Test User");

		// Verify domain user has avatar linked
		var updatedDomainUser = await _userRepository.GetByIdentityUserIdAsync(user.Id);
		updatedDomainUser.Should().NotBeNull();
		updatedDomainUser!.AvatarId.Should().Be(mediaImage.Id);
	}

	[Fact]
	public async Task UpdateProfilePictureAsync_ShouldCallImageProcessing()
	{
		// Arrange
		var user = new ApplicationUser { Email = "user@test.com", UserName = "user" };
		await _userManager.CreateAsync(user, "Password123!");
		var domainUser = new User(user.Id, "John", "Doe");
		_userRepository.Add(domainUser);
		await _dbContext.SaveChangesAsync();

		var fileStream = new MemoryStream(new byte[1024]);
		var fileName = "photo.png";
		var contentType = "image/png";

		// Act
		await _userService.UpdateProfilePictureAsync(user.Id, fileStream, fileName, contentType);

		// Assert - verify image processing was called with correct parameters
		_imageServiceMock.Verify(x => x.ProcessAsync(
			fileStream,
			ImageResizeMode.Thumbnail,
			256,
			256,
			It.IsAny<CancellationToken>()
		), Times.Once);
	}

	[Fact]
	public async Task UpdateProfilePictureAsync_ShouldUploadToFileStorage()
	{
		// Arrange
		var user = new ApplicationUser { Email = "user@test.com", UserName = "user" };
		await _userManager.CreateAsync(user, "Password123!");
		var domainUser = new User(user.Id, "Jane", "Smith");
		_userRepository.Add(domainUser);
		await _dbContext.SaveChangesAsync();

		var fileStream = new MemoryStream(new byte[1024]);
		var fileName = "avatar.jpg";
		var contentType = "image/jpeg";

		// Act
		await _userService.UpdateProfilePictureAsync(user.Id, fileStream, fileName, contentType);

		// Assert - verify upload was called
		_fileStorageMock.Verify(x => x.UploadAsync(
			It.IsAny<Stream>(),
			fileName,
			"image/webp", // processed content type
			It.IsAny<CancellationToken>()
		), Times.Once);
	}

	[Fact]
	public async Task UpdateProfilePictureAsync_WithExistingAvatar_ShouldDeleteOldAvatar()
	{
		// Arrange - create user with existing avatar
		var user = new ApplicationUser { Email = "user@test.com", UserName = "user" };
		await _userManager.CreateAsync(user, "Password123!");

		var oldAvatar = new MediaImage("old-avatar.webp", "image/webp", 128, 128, "Old Avatar");
		_mediaImageRepository.Add(oldAvatar);

		var domainUser = new User(user.Id, "Bob", "Johnson");
		domainUser.SetAvatar(oldAvatar);
		_userRepository.Add(domainUser);
		await _dbContext.SaveChangesAsync();

		var fileStream = new MemoryStream(new byte[1024]);

		// Act
		await _userService.UpdateProfilePictureAsync(user.Id, fileStream, "new-avatar.jpg", "image/jpeg");

		// Assert - verify old file was deleted from storage
		_fileStorageMock.Verify(x => x.DeleteAsync("old-avatar.webp", It.IsAny<CancellationToken>()), Times.Once);

		// Verify new avatar is set
		var updatedUser = await _userRepository.GetByIdentityUserIdAsync(user.Id);
		updatedUser!.Avatar!.StorageKey.Should().Be("test-storage-key.webp");
	}

	[Fact]
	public async Task UpdateProfilePictureAsync_WhenUserNotFound_ReturnsNull()
	{
		// Arrange
		var nonExistentUserId = Guid.NewGuid();
		var fileStream = new MemoryStream(new byte[1024]);

		// Act
		var result = await _userService.UpdateProfilePictureAsync(nonExistentUserId, fileStream, "avatar.jpg", "image/jpeg");

		// Assert
		result.Should().BeNull();

		// Verify no MediaImage was created
		var mediaImages = await _mediaImageRepository.GetAllAsync();
		mediaImages.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateProfilePictureAsync_ShouldCreateCorrectForeignKeyRelationship()
	{
		// Arrange
		var user = new ApplicationUser { Email = "fk@test.com", UserName = "fkuser" };
		await _userManager.CreateAsync(user, "Password123!");
		var domainUser = new User(user.Id, "FK", "Test");
		_userRepository.Add(domainUser);
		await _dbContext.SaveChangesAsync();

		var fileStream = new MemoryStream(new byte[1024]);

		// Act
		await _userService.UpdateProfilePictureAsync(user.Id, fileStream, "avatar.jpg", "image/jpeg");

		// Assert - verify FK relationship
		var updatedUser = await _dbContext.DomainUsers
			.Include(u => u.Avatar)
			.FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

		updatedUser.Should().NotBeNull();
		updatedUser!.AvatarId.Should().NotBeNull();
		updatedUser.Avatar.Should().NotBeNull();
		updatedUser.Avatar!.Id.Should().Be(updatedUser.AvatarId.Value);
	}

	[Fact]
	public async Task UpdateProfilePictureAsync_ShouldSaveChangesToDatabase()
	{
		// Arrange
		var user = new ApplicationUser { Email = "save@test.com", UserName = "saveuser" };
		await _userManager.CreateAsync(user, "Password123!");
		var domainUser = new User(user.Id, "Save", "Test");
		_userRepository.Add(domainUser);
		await _dbContext.SaveChangesAsync();

		var fileStream = new MemoryStream(new byte[1024]);

		// Get initial state
		var imagesBefore = await _mediaImageRepository.GetAllAsync();
		imagesBefore.Should().BeEmpty();

		// Act
		await _userService.UpdateProfilePictureAsync(user.Id, fileStream, "avatar.jpg", "image/jpeg");

		// Assert - verify changes persisted
		var imagesAfter = await _mediaImageRepository.GetAllAsync();
		imagesAfter.Should().HaveCount(1);

		var userFromDb = await _dbContext.DomainUsers.FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);
		userFromDb!.AvatarId.Should().NotBeNull();
	}

	[Fact]
	public async Task UpdateProfilePictureAsync_MultipleUploads_ShouldOnlyKeepLatestAvatar()
	{
		// Arrange
		var user = new ApplicationUser { Email = "multi@test.com", UserName = "multiuser" };
		await _userManager.CreateAsync(user, "Password123!");
		var domainUser = new User(user.Id, "Multi", "Upload");
		_userRepository.Add(domainUser);
		await _dbContext.SaveChangesAsync();

		// Setup mock to return different keys
		var uploadCount = 0;
		_fileStorageMock
			.Setup(x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(() => $"avatar-{++uploadCount}.webp");

		// Act - upload three times
		await _userService.UpdateProfilePictureAsync(user.Id, new MemoryStream(new byte[100]), "avatar1.jpg", "image/jpeg");
		await _userService.UpdateProfilePictureAsync(user.Id, new MemoryStream(new byte[100]), "avatar2.jpg", "image/jpeg");
		await _userService.UpdateProfilePictureAsync(user.Id, new MemoryStream(new byte[100]), "avatar3.jpg", "image/jpeg");

		// Assert - verify only latest avatar is referenced
		var updatedUser = await _userRepository.GetByIdentityUserIdAsync(user.Id);
		updatedUser!.Avatar!.StorageKey.Should().Be("avatar-3.webp");

		// Verify old files were deleted
		_fileStorageMock.Verify(x => x.DeleteAsync("avatar-1.webp", It.IsAny<CancellationToken>()), Times.Once);
		_fileStorageMock.Verify(x => x.DeleteAsync("avatar-2.webp", It.IsAny<CancellationToken>()), Times.Once);

		// Verify all MediaImages are in database (not cleaned up automatically)
		var allImages = await _mediaImageRepository.GetAllAsync();
		allImages.Should().HaveCount(3);
	}
}
