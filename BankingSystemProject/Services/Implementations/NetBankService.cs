using BankingSystem.Core.DTOs;
using BankingSystem.Services.Interfaces;
using BankingSystemProject.Data;
using BankingSystemProject.Data.Tables;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BankingSystemProject.Services.Implementations
{
    public class NetBankService : INetBankService
    {
        private readonly BankingDbContext context;

        public NetBankService(BankingDbContext context)
        {
            this.context = context;
        }
        public async Task<List<BankAccount>> GetBankAccountsForUserAsync(string userId)
        {   
            return await context.BankAccounts.Where(account => account.UserId == userId).ToListAsync();
        }

        public async Task<List<Card>> GetCardsForUserAsync(string userId)
        {
            return await context.Cards
                .Join(
                    context.BankAccounts,
                    card => card.BankAccountId,
                    bankAccount => bankAccount.Id,
                    (card, bankAccount) => new { Card = card, BankAccount = bankAccount })
                .Where(joinResult => joinResult.BankAccount.UserId == userId)
                .Select(joinResult => joinResult.Card)
                .ToListAsync();
        }

        public async Task CreateTransactionAsync(TransactionCreateDTO transactionCreateDTO)
        {
            try
            {
                var senderAccount = await context.BankAccounts
                    .Include(ba => ba.User)
                    .Where(x => x.IBAN == transactionCreateDTO.SenderBankAccountIBAN)
                    .FirstOrDefaultAsync();

                var receiverAccount = await context.BankAccounts
                    .Where(x => x.IBAN == transactionCreateDTO.ReceiverBankAccountIBAN)
                    .FirstOrDefaultAsync();

                if (senderAccount == null || receiverAccount == null)
                {
                    throw new Exception("Sender or receiver account not found.");
                }

                // განსაზღვრავს გამგზავნი და მიმღები ანგარიშების ვალუტებს
                var senderCurrency = senderAccount.CurrencyCode;
                var receiverCurrency = receiverAccount.CurrencyCode;

                decimal amountToTransfer = transactionCreateDTO.Amount;
                decimal commissionRate = 0.01m; // 1%
                decimal commissionAmount = 0;

                if (senderAccount.UserId == receiverAccount.UserId)
                {
                    // საკუთარ ანგარიშზე გადარიცხვა
                    // საკომისიოს გარეშე
                    if (senderAccount.Amount < amountToTransfer)
                    {
                        throw new Exception("Insufficient balance.");
                    }

                    if (senderCurrency != receiverCurrency)
                    {
                        // თუ გამგზავნი და მიმღები ვალუტები ერთმანეთს არ ეთხვევა, საჭიროა კონვერტაცია
                        // ეძებს გაცვლით კურსს ბაზიდან, რომელიც შეესაბამება გამგზავნი და მიმღები ანგარიშების კურსებს
                        var exchangeRate = await context.ExchangeRates
                            .Where(rate => rate.FromCurrencyCode == senderCurrency && rate.ToCurrencyCode == receiverCurrency)
                            .FirstOrDefaultAsync() ?? throw new Exception("Exchange rate not found.");
                        decimal rate = exchangeRate.Rate;
                        decimal convertedAmount = amountToTransfer * rate;

                        senderAccount.Amount -= amountToTransfer;
                        receiverAccount.Amount += convertedAmount;
                    }
                    else
                    {
                        // თუ კურსები არ განსხვავდება კონვერტაცია საჭირო არაა
                        senderAccount.Amount -= amountToTransfer;
                        receiverAccount.Amount += amountToTransfer;
                    }
                }
                else
                {
                    // თანხის გადარიცხვა სხვის ანგარიშზე
                    // საჭიროა საკომისიოს გათვალისწინება
                    commissionAmount = amountToTransfer * commissionRate + 0.5m;

                    if (senderAccount.Amount < amountToTransfer + commissionAmount)
                    {
                        throw new Exception("Insufficient balance.");
                    }

                    if (senderCurrency != receiverCurrency)
                    {
                        // თუ გამგზავნი და მიმღები ვალუტები ერთმანეთს არ ეთხვევა, საჭიროა კონვერტაცია
                        // ეძებს გაცვლით კურსს ბაზიდან, რომელიც შეესაბამება გამგზავნი და მიმღები ანგარიშების კურსებს
                        var exchangeRate = await context.ExchangeRates
                            .Where(rate => rate.FromCurrencyCode == senderCurrency && rate.ToCurrencyCode == receiverCurrency)
                            .FirstOrDefaultAsync() ?? throw new Exception("Exchange rate not found.");
                        decimal rate = exchangeRate.Rate;
                        decimal convertedAmount = amountToTransfer * rate;

                        senderAccount.Amount -= amountToTransfer + commissionAmount;
                        receiverAccount.Amount += convertedAmount;
                    }
                    else
                    {
                        // თუ კურსები არ განსხვავდება კონვერტაცია საჭირო არაა
                        senderAccount.Amount -= amountToTransfer + commissionAmount;
                        receiverAccount.Amount += amountToTransfer;
                    }
                }

                // ტრანზაქციის შექმნა, რადგან შედარებით რთული ობიექტია, არ ვიყენებ AutoMapper-ს
                var transaction = new Transaction
                {
                    Amount = amountToTransfer,
                    CommisionAmount = commissionAmount,
                    CurrencyCode = senderCurrency,
                    SenderBankAccountIBAN = senderAccount.IBAN,
                    ReceiverBankAccountIBAN = receiverAccount.IBAN,
                    SenderUserId = senderAccount.UserId
                };

                await context.Transactions.AddAsync(transaction);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "An error occurred while saving the entity changes.");
                throw new Exception("An error occurred while saving the entity changes.", ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred in the NetBankService.");
                throw;
            }
        }
    }
}