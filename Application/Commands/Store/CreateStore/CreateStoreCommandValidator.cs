using FluentValidation;

namespace Application.Commands.Store.CreateStore;

public sealed class CreateStoreCommandValidator : AbstractValidator<CreateStoreCommand>
{
	public CreateStoreCommandValidator()
	{
		RuleFor(x => x.UserId).NotEmpty();
		RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
		RuleFor(x => x.Description).MaximumLength(2000);
	}
}
