using BankingSystemProject.Core.Enums;

namespace BankingSystem.Core.DTOs
{
    public class BetweenDateCurrencyDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public CurrenciesEnum CurrencyCode { get; set; }
    }
}
