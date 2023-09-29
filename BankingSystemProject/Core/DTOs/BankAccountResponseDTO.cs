namespace BankingSystemProject.Core.DTOs
{
    public class BankAccountResponseDTO
    {
        public string IBAN { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
