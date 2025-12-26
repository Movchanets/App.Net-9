using FluentValidation;

namespace Application.Commands.Tag.UpdateTag;

public sealed class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
	public UpdateTagCommandValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty()
			.WithMessage("Id is required");

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
