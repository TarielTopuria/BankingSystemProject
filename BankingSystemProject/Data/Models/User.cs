using BankingSystemProject.Data.Tables;
using Microsoft.AspNetCore.Identity;

namespace BankingSystemProject.Data.Models
{
    //User მოდელს ვიყენებ, რათა IdentityUser თეიბლს დავამატო სასურველი ქოლამები
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonalNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }
        public List<BankAccount> BankAccounts { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
