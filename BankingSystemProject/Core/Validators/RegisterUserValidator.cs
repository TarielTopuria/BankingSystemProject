using BankingSystemProject.Core.DTOs;
using FluentValidation;

namespace BankingSystemProject.Core.Validators
{
    public class RegisterUserValidator : AbstractValidator<RegisterUserDTO>
    {
        public RegisterUserValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.PersonalNumber).NotEmpty().Length(11).Must(BeNumeric);
            RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateTime.Now);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one numeric digit.")
                .Matches("[!@#$%^&*]").WithMessage("Password must contain at least one special character (!@#$%^&*).");
            RuleFor(x => x.Role).NotEmpty().IsInEnum().WithMessage("Role must be enum 'Admin' = 1, 'Manager' = 2, 'Operator' = 3, 'Client' = 4");
        }

        private bool BeNumeric(string value)
        {
            return value.All(char.IsDigit);
        }
    }
}
