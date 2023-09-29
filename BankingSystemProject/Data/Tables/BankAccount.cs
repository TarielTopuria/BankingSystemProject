using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using BankingSystemProject.Core.Enums;
using BankingSystemProject.Data.Models;

namespace BankingSystemProject.Data.Tables
{
    [Table("BankAccounts")]
    public class BankAccount
    {
        [Key]
        public int Id { get; set; }
        public string IBAN { get; set; }
        public decimal Amount { get; set; }
        public CurrenciesEnum CurrencyCode { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public List<Card> Cards { get; set; }
    }
}
