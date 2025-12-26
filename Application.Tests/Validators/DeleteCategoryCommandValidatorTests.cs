using Application.Commands.Category.DeleteCategory;
using FluentAssertions;

namespace Application.Tests.Validators;

// There is no dedicated validator for DeleteCategoryCommand currently.
// This test ensures the command shape is still usable.
public class DeleteCategoryCommandValidatorTests
{
	[Fact]
	public void Command_WithEmptyId_IsStillConstructible()
	{
		var cmd = new DeleteCategoryCommand(Guid.Empty);
		cmd.Id.Should().Be(Guid.Empty);
	}
}
