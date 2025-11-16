using Application.Validators;
using Application.ViewModels;
using FluentAssertions;
using Xunit;

namespace Application.Tests.Validators;

public class UpdateUserVMValidatorTests
{
	private readonly UpdateUserVMValidator _validator = new UpdateUserVMValidator();

	[Fact]
	public void Validate_ValidModel_NoErrors()
	{
		var vm = new UpdateUserVM
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
		var vm = new UpdateUserVM { Email = "not-an-email" };
		var result = _validator.Validate(vm);
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e => e.PropertyName == "Email");
	}

	
}
