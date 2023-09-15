using BankingSystemProject.Core.Enums;

namespace BankingSystem.Core.DTOs
{
    public class TransactionCreateDTO
    {
        public decimal Amount { get; set; }
        public Currencies CurrencyCode { get; set; }
        public string SenderBankAccountIBAN { get; set; }
        public string ReceiverBankAccountIBAN { get; set; }
    }
}
