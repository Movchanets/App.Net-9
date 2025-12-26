using FluentValidation;

namespace Application.Commands.Category.UpdateCategory;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
	public UpdateCategoryCommandValidator()
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

		RuleFor(x => x.ParentCategoryId)
			.Must(id => id == null || id.Value != Guid.Empty)
			.WithMessage("ParentCategoryId must be a valid GUID");
	}
}
