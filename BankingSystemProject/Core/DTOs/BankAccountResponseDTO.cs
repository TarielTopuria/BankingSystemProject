using BankingSystemProject.Core.Enums;

namespace BankingSystemProject.Core.DTOs
{
    public class BankAccountResponseDTO
    {
        public string IBAN { get; set; }
        public decimal Amount { get; set; }
        public Currencies CurrencyCode { get; set; }
    }
}
