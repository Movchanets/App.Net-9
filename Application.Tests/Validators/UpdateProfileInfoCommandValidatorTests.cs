using Application.Commands.User.Profile.UpdateProfileInfo;
using Application.DTOs;
using Application.Validators;
using FluentAssertions;
using System;
using Xunit;

namespace Application.Tests.Validators;

public class UpdateProfileInfoCommandValidatorTests
{
	private readonly UpdateProfileInfoCommandValidator _validator = new();

	[Fact]
	public void Validate_ValidUsername_PassesValidation()
	{
		var command = new UpdateProfileInfoCommand(Guid.NewGuid(), new UpdateProfileInfoDto { Username = "john.doe" });
		var result = _validator.Validate(command);
		result.IsValid.Should().BeTrue();
	}

	[Theory]
	[InlineData("ab")] // too short
	[InlineData("a")] // too short
	[InlineData("user@name")] // invalid character
	[InlineData("user name")] // space not allowed
	public void Validate_InvalidUsername_FailsValidation(string username)
	{
		var command = new UpdateProfileInfoCommand(Guid.NewGuid(), new UpdateProfileInfoDto { Username = username });
		var result = _validator.Validate(command);
		result.IsValid.Should().BeFalse();
	}

	[Fact]
	public void Validate_NullUsername_PassesValidation()
	{
		var command = new UpdateProfileInfoCommand(Guid.NewGuid(), new UpdateProfileInfoDto { Username = null });
		var result = _validator.Validate(command);
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_NameTooLong_FailsValidation()
	{
		var longName = new string('a', 51);
		var vm = new UpdateProfileInfoDto { Name = longName };
		var command = new UpdateProfileInfoCommand(Guid.NewGuid(), vm);
		var result = _validator.Validate(command);
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Data.Name");
	}

	[Fact]
	public void Validate_SurnameTooLong_FailsValidation()
	{
		var longSurname = new string('b', 51);
		var vm = new UpdateProfileInfoDto { Surname = longSurname };
		var command = new UpdateProfileInfoCommand(Guid.NewGuid(), vm);
		var result = _validator.Validate(command);
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Data.Surname");
	}

	[Fact]
	public void Validate_AllFieldsValid_PassesValidation()
	{
		var vm = new UpdateProfileInfoDto
		{
			Name = "John",
			Surname = "Doe",
			Username = "john.doe"
		};
		var command = new UpdateProfileInfoCommand(Guid.NewGuid(), vm);
		var result = _validator.Validate(command);
		result.IsValid.Should().BeTrue();
	}
}
