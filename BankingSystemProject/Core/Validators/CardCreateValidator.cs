using BankingSystemProject.Core.DTOs;
using FluentValidation;

namespace BankingSystemProject.Core.Validators
{
    public class CardCreateValidator : AbstractValidator<CardCreateDTO>
    {
        public CardCreateValidator()
        {
            RuleFor(card => card.CardNumber)
            .NotEmpty()
            .Must(BeNumeric)
            .WithMessage("CardNumber must contain 16 numeric values")
            .Length(16);

            RuleFor(card => card.CardHolder)
                .NotEmpty()
                .Must(BeNonNumeric)
                .WithMessage("CardHolder must not contain numeric values");

            RuleFor(card => card.ExpirationDate)
                .NotEmpty()
                .GreaterThanOrEqualTo(DateTime.UtcNow)
                .LessThanOrEqualTo(DateTime.UtcNow.AddYears(10));

            RuleFor(card => card.CVV).NotEmpty().Length(3).Must(BeNumeric).WithMessage("CVV must be numeric");
            RuleFor(card => card.PIN).NotEmpty().Length(4).Must(BeNumeric).WithMessage("PIN must be numeric");
        }

        private bool BeNumeric(string value)
        {
            return value.All(char.IsDigit);
        }

        private bool BeNonNumeric(string value)
        {
            return !value.Any(char.IsDigit);
        }
    }
}
