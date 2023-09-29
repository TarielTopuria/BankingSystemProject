using BankingSystemProject.Core.Enums;
using BankingSystemProject.Data.Models;
using BankingSystemProject.Data.Tables;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BankingSystemProject.Data
{
    // მშობელ კლასად განვსაზღვრავ User-ში დაკონკრეტებულ ტიპს, რადგან ბაზა დავაკონფიგურო JWT Identity-ს გამოყენებით
    public class BankingDbContext : IdentityDbContext<User>
    {
        public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options){}

        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<Withdrawal> Withdrawals { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //თეიბლებს შორის რელაციების სირთულის გამო ბაზა დაკონფიგურირებულია ჰარდად
            base.OnModelCreating(modelBuilder);
            EnsureDefaultRoles(modelBuilder);

            modelBuilder.Entity<User>()
                .HasMany(u => u.BankAccounts)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId);

            modelBuilder.Entity<BankAccount>()
                .HasMany(b => b.Cards)
                .WithOne(c => c.BankAccount)
                .HasForeignKey(c => c.BankAccountId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Transactions)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.SenderUserId);
        }

        private void EnsureDefaultRoles(ModelBuilder modelBuilder)
        {
            string[] defaultRoles = Enum.GetNames(typeof(RolesEnum));

            foreach (var roleName in defaultRoles)
            {
                modelBuilder.Entity<IdentityRole>().HasData(new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                });
            }
        }
    }
}
