using Application.Commands.User.Profile.UpdatePhone;
using Application.Validators;
using Application.ViewModels;
using FluentAssertions;
using System;
using Xunit;

namespace Application.Tests.Validators;

public class UpdatePhoneCommandValidatorTests
{
	private readonly UpdatePhoneCommandValidator _validator = new();

	[Theory]
	[InlineData("+380123456789")]
	[InlineData("+1234567890")]
	[InlineData("123-456-7890")]
	[InlineData("(123) 456 7890")]
	public void Validate_ValidPhoneNumber_PassesValidation(string phone)
	{
		var command = new UpdatePhoneCommand(Guid.NewGuid(), new UpdatePhoneVM { PhoneNumber = phone });
		var result = _validator.Validate(command);
		result.IsValid.Should().BeTrue();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("123")] // too short
	[InlineData("abc123456789")] // letters
	public void Validate_InvalidPhoneNumber_FailsValidation(string phone)
	{
		var command = new UpdatePhoneCommand(Guid.NewGuid(), new UpdatePhoneVM { PhoneNumber = phone });
		var result = _validator.Validate(command);
		result.IsValid.Should().BeFalse();
	}

	[Fact]
	public void Validate_EmptyPhoneNumber_ReturnsRequiredError()
	{
		var command = new UpdatePhoneCommand(Guid.NewGuid(), new UpdatePhoneVM { PhoneNumber = "" });
		var result = _validator.Validate(command);
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.ErrorMessage.Contains("required"));
	}

	[Fact]
	public void Validate_InvalidFormat_ReturnsFormatError()
	{
		var command = new UpdatePhoneCommand(Guid.NewGuid(), new UpdatePhoneVM { PhoneNumber = "abc" });
		var result = _validator.Validate(command);
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Invalid phone number format"));
	}
}
