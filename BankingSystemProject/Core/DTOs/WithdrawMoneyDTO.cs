using BankingSystemProject.Core.Enums;

namespace BankingSystemProject.Core.DTOs
{
    public class WithdrawMoneyDTO
    {
        public string CardNumber { get; set; }
        public decimal Amount { get; set; }
        public Currencies CurrencyCode { get ; set; }
    }
}
