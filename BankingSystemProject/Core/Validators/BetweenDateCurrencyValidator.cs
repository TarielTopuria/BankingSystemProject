using BankingSystem.Core.DTOs;
using FluentValidation;

namespace BankingSystemProject.Core.Validators
{
    public class BetweenDateCurrencyValidator : AbstractValidator<BetweenDateCurrencyDTO>
    {
        public BetweenDateCurrencyValidator()
        {
            RuleFor(account => account.FromDate).NotEmpty().LessThan(DateTime.Now);
            RuleFor(account => account.ToDate).NotEmpty();
            RuleFor(account => account.CurrencyCode).NotEmpty().IsInEnum().WithMessage("Currency must be enum 'GEL' = 1, 'USD' = 2, 'EUR' = 3");
        }
    }
}
