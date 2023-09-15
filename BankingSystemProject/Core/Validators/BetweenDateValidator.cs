using BankingSystem.Core.DTOs;
using FluentValidation;

namespace BankingSystemProject.Core.Validators
{
    public class BetweenDateValidator : AbstractValidator<BetweenDateDTO>
    {
        public BetweenDateValidator()
        {
            RuleFor(account => account.FromDate).NotEmpty().LessThan(DateTime.Now);
            RuleFor(account => account.ToDate).NotEmpty();
        }
    }
}
