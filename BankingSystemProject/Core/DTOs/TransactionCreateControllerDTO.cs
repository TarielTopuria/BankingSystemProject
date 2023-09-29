using BankingSystemProject.Core.Enums;

namespace BankingSystemProject.Core.DTOs
{
    public class TransactionCreateControllerDTO
    {
        public decimal Amount { get; set; }
        public CurrenciesEnum CurrencyCode { get; set; }
        public string SenderBankAccountIBAN { get; set; }
        public string ReceiverBankAccountIBAN { get; set; }
    }
}
