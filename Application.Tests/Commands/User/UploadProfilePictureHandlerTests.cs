using Application.Commands.User.Profile.UploadProfilePicture;
using Application.DTOs;
using Application.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Xunit;

namespace Application.Tests.Commands.User;

public class UploadProfilePictureHandlerTests
{
	private readonly Mock<IUserService> _userServiceMock = new();
	private readonly Mock<ILogger<UploadProfilePictureHandler>> _loggerMock = new();

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUploadSucceeds_ReturnsSuccessWithDto()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var fileName = "avatar.jpg";
		var contentType = "image/jpeg";
		var fileStream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF }); // Fake JPEG header

		var expectedDto = new UserDto(
			userId,
			"testuser",
			"John",
			"Doe",
			"john@example.com",
			"+123456789",
			new List<string> { "User" },
			"/uploads/avatar.webp"
		);

		_userServiceMock
			.Setup(s => s.UpdateProfilePictureAsync(userId, fileStream, fileName, contentType, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedDto);

		var handler = new UploadProfilePictureHandler(_userServiceMock.Object, _loggerMock.Object);
		var command = new UploadProfilePictureCommand(userId, fileStream, fileName, contentType);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Payload.Should().NotBeNull();
		result.Payload!.Id.Should().Be(userId);
		result.Payload!.AvatarUrl.Should().Be("/uploads/avatar.webp");
		result.Message.Should().Be("Profile picture uploaded successfully");

		_userServiceMock.Verify(s => s.UpdateProfilePictureAsync(
			userId,
			fileStream,
			fileName,
			contentType,
			It.IsAny<CancellationToken>()), Times.Once);
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenUserNotFound_ReturnsFailure()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var fileName = "avatar.jpg";
		var contentType = "image/jpeg";
		var fileStream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF });

		_userServiceMock
			.Setup(s => s.UpdateProfilePictureAsync(userId, fileStream, fileName, contentType, It.IsAny<CancellationToken>()))
			.ReturnsAsync((UserDto?)null);

		var handler = new UploadProfilePictureHandler(_userServiceMock.Object, _loggerMock.Object);
		var command = new UploadProfilePictureCommand(userId, fileStream, fileName, contentType);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Message.Should().Be("Profile picture upload failed");
		result.Payload.Should().BeNull();
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_WhenExceptionThrown_ReturnsFailureWithErrorMessage()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var fileName = "avatar.jpg";
		var contentType = "image/jpeg";
		var fileStream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF });
		var exceptionMessage = "Storage service unavailable";

		_userServiceMock
			.Setup(s => s.UpdateProfilePictureAsync(userId, fileStream, fileName, contentType, It.IsAny<CancellationToken>()))
			.ThrowsAsync(new InvalidOperationException(exceptionMessage));

		var handler = new UploadProfilePictureHandler(_userServiceMock.Object, _loggerMock.Object);
		var command = new UploadProfilePictureCommand(userId, fileStream, fileName, contentType);

		// Act
		var result = await handler.Handle(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeFalse();
		result.Message.Should().Contain(exceptionMessage);
		result.Payload.Should().BeNull();

		_loggerMock.Verify(
			x => x.Log(
				LogLevel.Error,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => true),
				It.IsAny<Exception>(),
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
	}

	[Fact]
	public async System.Threading.Tasks.Task Handle_LogsInformationOnSuccess()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var fileName = "avatar.jpg";
		var contentType = "image/jpeg";
		var fileStream = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF });

		var expectedDto = new UserDto(
			userId,
			"testuser",
			"John",
			"Doe",
			"john@example.com",
			"+123456789",
			new List<string> { "User" },
			"/uploads/avatar.webp"
		);

		_userServiceMock
			.Setup(s => s.UpdateProfilePictureAsync(userId, fileStream, fileName, contentType, It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedDto);

		var handler = new UploadProfilePictureHandler(_userServiceMock.Object, _loggerMock.Object);
		var command = new UploadProfilePictureCommand(userId, fileStream, fileName, contentType);

		// Act
		await handler.Handle(command, CancellationToken.None);

		// Assert
		_loggerMock.Verify(
			x => x.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Uploading profile picture")),
				null,
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);

		_loggerMock.Verify(
			x => x.Log(
				LogLevel.Information,
				It.IsAny<EventId>(),
				It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully uploaded profile picture")),
				null,
				It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
			Times.Once);
	}
}
