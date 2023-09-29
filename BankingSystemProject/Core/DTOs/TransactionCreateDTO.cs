using BankingSystemProject.Core.Enums;

namespace BankingSystem.Core.DTOs
{
    public class TransactionCreateDTO
    {
        public decimal Amount { get; set; }
        public CurrenciesEnum CurrencyCode { get; set; }
        public string SenderBankAccountIBAN { get; set; }
        public string ReceiverBankAccountIBAN { get; set; }
        public string UserId { get; set; }
    }
}
