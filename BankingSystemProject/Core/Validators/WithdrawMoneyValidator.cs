using BankingSystemProject.Core.DTOs;
using FluentValidation;

namespace BankingSystemProject.Core.Validators
{
    public class WithdrawMoneyValidator : AbstractValidator<WithdrawMoneyControllerDTO>
    {
        public WithdrawMoneyValidator()
        {
            RuleFor(x => x.Amount).NotEmpty().GreaterThan(0);
            RuleFor(x => x.CurrencyCode).NotEmpty().IsInEnum().WithMessage("Currency must be enum 'GEL' = 1, 'USD' = 2, 'EUR' = 3");
        }
    }
}
