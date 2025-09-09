using Application.Commands.User.CreateUser;
using FluentValidation;

namespace Application.Validators;

public class CreateUserValidator : AbstractValidator<RegisterUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.data.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(50);
        RuleFor(x => x.data.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress();
    }
}