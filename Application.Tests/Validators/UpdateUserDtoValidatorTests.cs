using Application.DTOs;
using Application.Validators;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Validators;

public class UpdateUserDtoValidatorTests
{
	private readonly UpdateUserDtoValidator _validator = new UpdateUserDtoValidator();

	[Fact]
	public void Validate_ValidModel_NoErrors()
	{
		var vm = new UpdateUserDto
		{
			Name = "John",
			Surname = "Doe",
			Email = "john@example.com",
		};

		var result = _validator.Validate(vm);
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_InvalidEmail_ReturnsError()
	{
		var vm = new UpdateUserDto { Email = "not-an-email" };
		var result = _validator.Validate(vm);
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e => e.PropertyName == "Email");
	}


}
