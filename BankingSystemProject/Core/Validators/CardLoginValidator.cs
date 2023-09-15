using BankingSystemProject.Core.DTOs;
using FluentValidation;

namespace BankingSystemProject.Core.Validators
{
    public class CardLoginValidator : AbstractValidator<CardLoginDTO>
    {
        public CardLoginValidator()
        {
            RuleFor(card => card.CardNumber)
            .NotEmpty()
            .Must(BeNumeric)
            .WithMessage("CardNumber must contain 16 numeric values")
            .Length(16);

            RuleFor(card => card.PIN).NotEmpty().Length(4);
        }

        private bool BeNumeric(string value)
        {
            return value.All(char.IsDigit);
        }
    }
}
