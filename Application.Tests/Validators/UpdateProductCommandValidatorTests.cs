using Application.Commands.Product.UpdateProduct;
using FluentAssertions;

namespace Application.Tests.Validators;

public class UpdateProductCommandValidatorTests
{
	[Fact]
	public void Validate_WhenProductIdEmpty_ShouldFail()
	{
		var validator = new UpdateProductCommandValidator();
		var result = validator.Validate(new UpdateProductCommand(Guid.NewGuid(), Guid.Empty, "P", null, Guid.NewGuid()));
		result.IsValid.Should().BeFalse();
	}

	[Fact]
	public void Validate_WhenValid_ShouldPass()
	{
		var validator = new UpdateProductCommandValidator();
		var result = validator.Validate(new UpdateProductCommand(Guid.NewGuid(), Guid.NewGuid(), "P", null, Guid.NewGuid()));
		result.IsValid.Should().BeTrue();
	}
}
