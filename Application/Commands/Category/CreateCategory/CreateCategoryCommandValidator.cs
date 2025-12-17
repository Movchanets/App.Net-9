using FluentValidation;

namespace Application.Commands.Category.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
	public CreateCategoryCommandValidator()
	{
		RuleFor(x => x.Name)
			.NotEmpty()
			.MaximumLength(200);

		RuleFor(x => x.Description)
			.MaximumLength(2000);

		RuleFor(x => x.ParentCategoryId)
			.Must(id => id == null || id.Value != Guid.Empty)
			.WithMessage("ParentCategoryId must be a non-empty GUID.");
	}
}
