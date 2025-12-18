using Application.Commands.Store.UpdateStore;
using FluentAssertions;

namespace Application.Tests.Validators;

public class UpdateStoreCommandValidatorTests
{
	[Fact]
	public void Validate_WhenUserIdEmpty_ShouldFail()
	{
		var validator = new UpdateStoreCommandValidator();
		var result = validator.Validate(new UpdateStoreCommand(Guid.Empty, "Store", null));
		result.IsValid.Should().BeFalse();
	}

	[Fact]
	public void Validate_WhenValid_ShouldPass()
	{
		var validator = new UpdateStoreCommandValidator();
		var result = validator.Validate(new UpdateStoreCommand(Guid.NewGuid(), "Store", null));
		result.IsValid.Should().BeTrue();
	}
}
