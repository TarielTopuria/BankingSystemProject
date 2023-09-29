using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using BankingSystemProject.Core.Enums;

namespace BankingSystemProject.Data.Tables
{
    [Table("ExchangeRates")]
    public class ExchangeRate
    {
        [Key]
        public int Id { get; set; }
        public CurrenciesEnum FromCurrencyCode { get; set; }
        public CurrenciesEnum ToCurrencyCode { get; set; }
        public decimal Rate { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
