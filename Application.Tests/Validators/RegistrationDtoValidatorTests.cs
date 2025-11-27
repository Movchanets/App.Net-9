using Application.DTOs;
using Application.Validators;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Validators;

public class RegistrationDtoValidatorTests
{
	private readonly RegistrationDtoValidator _validator = new RegistrationDtoValidator();

	[Fact]
	public void Validate_ValidModel_NoErrors()
	{
		var vm = new RegistrationDto
		{
			Email = "user@example.com",
			Name = "John",
			Surname = "Doe",
			Password = "secret123",
			ConfirmPassword = "secret123"
		};

		var result = _validator.Validate(vm);
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_MissingEmail_ReturnsError()
	{
		var vm = new RegistrationDto
		{
			Email = "",
			Name = "John",
			Surname = "Doe",
			Password = "secret123",
			ConfirmPassword = "secret123"
		};

		var result = _validator.Validate(vm);
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e => e.PropertyName == "Email");
	}

	[Fact]
	public void Validate_PasswordsDoNotMatch_ReturnsError()
	{
		var vm = new RegistrationDto
		{
			Email = "user@example.com",
			Name = "John",
			Surname = "Doe",
			Password = "secret123",
			ConfirmPassword = "different"
		};

		var result = _validator.Validate(vm);
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e => e.PropertyName == "ConfirmPassword");
	}
}
