using BankingSystemProject.Core.DTOs;
using FluentValidation;

namespace BankingSystemProject.Core.Validators
{
    public class LoginUserValidator : AbstractValidator<LoginUserDTO>
    {
        public LoginUserValidator()
        {
            RuleFor(account => account.UserName).NotEmpty();
            RuleFor(model => model.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one numeric digit.")
                .Matches("[!@#$%^&*]").WithMessage("Password must contain at least one special character (!@#$%^&*).");
        }
    }
}
