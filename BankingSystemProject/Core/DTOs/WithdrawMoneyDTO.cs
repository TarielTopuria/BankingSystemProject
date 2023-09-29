using BankingSystemProject.Core.Enums;

namespace BankingSystemProject.Core.DTOs
{
    public class WithdrawMoneyDTO
    {
        public string CardNumber { get; set; }
        public decimal Amount { get; set; }
        public CurrenciesEnum CurrencyCode { get ; set; }
    }
}
