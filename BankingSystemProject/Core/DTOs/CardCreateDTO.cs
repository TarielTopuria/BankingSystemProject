namespace BankingSystemProject.Core.DTOs
{
    public class CardCreateDTO
    {
        public string CardNumber { get; set; }
        public string CardHolder { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string CVV { get; set; }
        public string PIN { get; set; }
        public int BankAccountId { get; set; }
    }
}
