using BankingSystemProject.Core.Enums;

namespace BankingSystemProject.Core.DTOs
{
    public class BankAccountCreateDTO
    {
        public string IBAN { get; set; }
        public decimal Amount { get; set; }
        public Currencies CurrencyCode { get; set; }
        public string UserId { get; set; }
    }
}
