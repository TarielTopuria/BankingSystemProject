using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using BankingSystemProject.Core.Enums;
using BankingSystemProject.Data.Models;

namespace BankingSystemProject.Data.Tables
{
    [Table("Transactions")]
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public decimal CommisionAmount { get; set; }
        public Currencies CurrencyCode { get; set; }
        public string SenderBankAccountIBAN { get; set; }
        public string ReceiverBankAccountIBAN { get; set; }
        public DateTime TransactionCreateDate { get; set; } = DateTime.Now;

        [ForeignKey("Users")]
        public string SenderUserId { get; set; }
        public User User { get; set; }
    }
}
