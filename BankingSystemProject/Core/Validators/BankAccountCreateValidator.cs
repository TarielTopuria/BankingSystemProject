using BankingSystemProject.Core.DTOs;
using FluentValidation;

namespace BankingSystemProject.Core.Validators
{
    public class BankAccountCreateValidator : AbstractValidator<BankAccountCreateDTO>
    {
        public BankAccountCreateValidator()
        {
            RuleFor(account => account.IBAN)
                .NotEmpty()
                .Must(BeAValidIBAN).WithMessage("IBAN must follow \"^GE00TT\\d{16}$\" pattern");
            RuleFor(account => account.Amount).GreaterThanOrEqualTo(0);
            RuleFor(account => account.CurrencyCode).NotEmpty().IsInEnum().WithMessage("Currency must be enum 'GEL' = 1, 'USD' = 2, 'EUR' = 3");
            RuleFor(account => account.UserId).NotEmpty();
        }

        private bool BeAValidIBAN(string iban)
        {
            // ამოწმებს ჩანაწერის შედგება თუ არა 22 სიმბოლოსგან
            if (iban.Length != 22)
                return false;

            // ამოწმებს ჩანაწერი იწყება თუ არა 'GE'-ით
            if (!iban.Substring(0, 2).Equals("GE", StringComparison.OrdinalIgnoreCase))
                return false;

            // ამოწმებს 'GE'-ს მოყვება თუ არა '00'
            if (!iban.Substring(2, 2).Equals("00", StringComparison.OrdinalIgnoreCase))
                return false;

            // ამოწმებს '00'-ს მოყვება თუ არა 'TT'
            if (!iban.Substring(4, 2).Equals("TT", StringComparison.OrdinalIgnoreCase))
                return false;

            // ამოწმებს დანარჩენი სიმბოლოები არის თუ არა რიცხვითი
            for (int i = 6; i < iban.Length; i++)
            {
                if (!char.IsDigit(iban[i]))
                    return false;
            }
            return true;
        }
    }
}
