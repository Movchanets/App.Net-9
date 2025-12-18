using FluentValidation;

namespace Application.Commands.Store.UpdateStore;

public sealed class UpdateStoreCommandValidator : AbstractValidator<UpdateStoreCommand>
{
	public UpdateStoreCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
		RuleFor(x => x.Description).MaximumLength(2000);
	}
}
