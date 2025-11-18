using Application.Commands.User.Profile.UploadProfilePicture;
using FluentAssertions;
using System;
using System.IO;
using Xunit;

namespace Application.Tests.Validators;

public class UploadProfilePictureValidatorTests
{
	private readonly UploadProfilePictureValidator _validator = new();

	[Theory]
	[InlineData("avatar.jpg", "image/jpeg")]
	[InlineData("photo.jpeg", "image/jpeg")]
	[InlineData("picture.png", "image/png")]
	[InlineData("icon.gif", "image/gif")]
	[InlineData("image.webp", "image/webp")]
	public void Validate_ValidFileExtensionAndContentType_PassesValidation(string fileName, string contentType)
	{
		// Arrange
		var fileStream = new MemoryStream(new byte[1024]); // 1KB file
		var command = new UploadProfilePictureCommand(Guid.NewGuid(), fileStream, fileName, contentType);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Theory]
	[InlineData("document.pdf", "application/pdf")]
	[InlineData("video.mp4", "video/mp4")]
	[InlineData("archive.zip", "application/zip")]
	[InlineData("text.txt", "text/plain")]
	public void Validate_InvalidFileExtension_FailsValidation(string fileName, string contentType)
	{
		// Arrange
		var fileStream = new MemoryStream(new byte[1024]);
		var command = new UploadProfilePictureCommand(Guid.NewGuid(), fileStream, fileName, contentType);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.ErrorMessage.Contains("extensions"));
	}

	[Theory]
	[InlineData("avatar.jpg", "application/pdf")]
	[InlineData("photo.png", "text/plain")]
	[InlineData("image.gif", "video/mp4")]
	public void Validate_InvalidContentType_FailsValidation(string fileName, string contentType)
	{
		// Arrange
		var fileStream = new MemoryStream(new byte[1024]);
		var command = new UploadProfilePictureCommand(Guid.NewGuid(), fileStream, fileName, contentType);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.ErrorMessage.Contains("ContentType"));
	}

	[Fact]
	public void Validate_EmptyFileName_FailsValidation()
	{
		// Arrange
		var fileStream = new MemoryStream(new byte[1024]);
		var command = new UploadProfilePictureCommand(Guid.NewGuid(), fileStream, "", "image/jpeg");

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.ErrorMessage.Contains("FileName is required"));
	}

	[Fact]
	public void Validate_EmptyContentType_FailsValidation()
	{
		// Arrange
		var fileStream = new MemoryStream(new byte[1024]);
		var command = new UploadProfilePictureCommand(Guid.NewGuid(), fileStream, "avatar.jpg", "");

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.ErrorMessage.Contains("ContentType is required"));
	}

	[Fact]
	public void Validate_NullFileStream_FailsValidation()
	{
		// Arrange
		var command = new UploadProfilePictureCommand(Guid.NewGuid(), null!, "avatar.jpg", "image/jpeg");

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.ErrorMessage.Contains("File stream is required"));
	}

	[Fact]
	public void Validate_EmptyFileStream_FailsValidation()
	{
		// Arrange
		var fileStream = new MemoryStream(Array.Empty<byte>());
		var command = new UploadProfilePictureCommand(Guid.NewGuid(), fileStream, "avatar.jpg", "image/jpeg");

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.ErrorMessage.Contains("File stream cannot be empty"));
	}

	[Fact]
	public void Validate_FileSizeExceeds5MB_FailsValidation()
	{
		// Arrange - Create a 6MB file
		var fileStream = new MemoryStream(new byte[6 * 1024 * 1024]);
		var command = new UploadProfilePictureCommand(Guid.NewGuid(), fileStream, "avatar.jpg", "image/jpeg");

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.ErrorMessage.Contains("must not exceed 5 MB"));
	}

	[Fact]
	public void Validate_FileSizeExactly5MB_PassesValidation()
	{
		// Arrange - Create exactly 5MB file
		var fileStream = new MemoryStream(new byte[5 * 1024 * 1024]);
		var command = new UploadProfilePictureCommand(Guid.NewGuid(), fileStream, "avatar.jpg", "image/jpeg");

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_EmptyUserId_FailsValidation()
	{
		// Arrange
		var fileStream = new MemoryStream(new byte[1024]);
		var command = new UploadProfilePictureCommand(Guid.Empty, fileStream, "avatar.jpg", "image/jpeg");

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.ErrorMessage.Contains("UserId is required"));
	}

	[Theory]
	[InlineData("IMAGE.JPG", "IMAGE/JPEG")]
	[InlineData("PHOTO.PNG", "IMAGE/PNG")]
	public void Validate_UppercaseExtensionAndContentType_PassesValidation(string fileName, string contentType)
	{
		// Arrange
		var fileStream = new MemoryStream(new byte[1024]);
		var command = new UploadProfilePictureCommand(Guid.NewGuid(), fileStream, fileName, contentType);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}
}
