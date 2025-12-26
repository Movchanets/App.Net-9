using FluentValidation;

namespace Application.Commands.Store.UpdateStore;

public sealed class UpdateStoreCommandValidator : AbstractValidator<UpdateStoreCommand>
{
	public UpdateStoreCommandValidator()
	{
		RuleFor(x => x.UserId)
			.NotEmpty()
			.WithMessage("UserId is required");

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
