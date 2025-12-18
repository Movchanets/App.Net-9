using Application.Commands.Store.CreateStore;
using FluentAssertions;

namespace Application.Tests.Validators;

public class CreateStoreCommandValidatorTests
{
	[Fact]
	public void Validate_WhenNameMissing_ShouldFail()
	{
		var validator = new CreateStoreCommandValidator();
		var result = validator.Validate(new CreateStoreCommand(Guid.NewGuid(), "", null));
		result.IsValid.Should().BeFalse();
	}

	[Fact]
	public void Validate_WhenValid_ShouldPass()
	{
		var validator = new CreateStoreCommandValidator();
		var result = validator.Validate(new CreateStoreCommand(Guid.NewGuid(), "Store", "desc"));
		result.IsValid.Should().BeTrue();
	}
}
