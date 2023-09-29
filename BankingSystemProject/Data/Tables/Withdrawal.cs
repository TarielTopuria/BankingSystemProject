using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using BankingSystemProject.Core.Enums;
using BankingSystemProject.Data.Models;

namespace BankingSystemProject.Data.Tables
{
    [Table("Withdrawals")]
    public class Withdrawal
    {
        [Key]
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal CommisionAmount { get; set; }
        public CurrenciesEnum CurrencyCode { get; set; }
        public DateTime WithdrawalCreateDate { get; set; } = DateTime.Now;

        [ForeignKey("Users")]
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
