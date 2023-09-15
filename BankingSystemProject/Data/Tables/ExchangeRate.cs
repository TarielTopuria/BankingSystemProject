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
        public Currencies FromCurrencyCode { get; set; }
        public Currencies ToCurrencyCode { get; set; }
        public decimal Rate { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
