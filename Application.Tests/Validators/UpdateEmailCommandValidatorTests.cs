using Application.Commands.User.Profile.UpdateEmail;
using Application.DTOs;
using Application.Validators;
using FluentAssertions;
using System;
using Xunit;

namespace Application.Tests.Validators;

public class UpdateEmailCommandValidatorTests
{
	private readonly UpdateEmailCommandValidator _validator = new();

	[Theory]
	[InlineData("user@example.com")]
	[InlineData("test.user@domain.co.uk")]
	[InlineData("john+doe@mail.org")]
	public void Validate_ValidEmail_PassesValidation(string email)
	{
		var command = new UpdateEmailCommand(Guid.NewGuid(), new UpdateEmailDto { Email = email });
		var result = _validator.Validate(command);
		result.IsValid.Should().BeTrue();
	}

	[Theory]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("invalid")]
	[InlineData("@example.com")]
	[InlineData("user@")]
	public void Validate_InvalidEmail_FailsValidation(string email)
	{
		var command = new UpdateEmailCommand(Guid.NewGuid(), new UpdateEmailDto { Email = email });
		var result = _validator.Validate(command);
		result.IsValid.Should().BeFalse();
	}

	[Fact]
	public void Validate_EmptyEmail_ReturnsRequiredError()
	{
		var command = new UpdateEmailCommand(Guid.NewGuid(), new UpdateEmailDto { Email = "" });
		var result = _validator.Validate(command);
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.ErrorMessage.Contains("required"));
	}
}
