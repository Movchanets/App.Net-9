using FluentValidation;

namespace Application.Commands.Tag.CreateTag;

public sealed class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
	public CreateTagCommandValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.WithMessage("Name is required")
			.MinimumLength(2)
			.WithMessage("Name must be at least 2 characters")
			.MaximumLength(200)
			.WithMessage("Name must not exceed 200 characters");

		RuleFor(x => x.Description)
			.MaximumLength(2000)
			.WithMessage("Description must not exceed 2000 characters");
	}
}
