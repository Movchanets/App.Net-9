using Application.Commands.Product.CreateProduct;
using FluentAssertions;

namespace Application.Tests.Validators;

public class CreateProductCommandValidatorTests
{
	[Fact]
	public void Validate_WhenPriceNegative_ShouldFail()
	{
		var validator = new CreateProductCommandValidator();
		var result = validator.Validate(new CreateProductCommand(Guid.NewGuid(), "P", null, Guid.NewGuid(), -1, 0));
		result.IsValid.Should().BeFalse();
	}

	[Fact]
	public void Validate_WhenValid_ShouldPass()
	{
		var validator = new CreateProductCommandValidator();
		var result = validator.Validate(new CreateProductCommand(Guid.NewGuid(), "P", "d", Guid.NewGuid(), 0, 0));
		result.IsValid.Should().BeTrue();
	}
}
