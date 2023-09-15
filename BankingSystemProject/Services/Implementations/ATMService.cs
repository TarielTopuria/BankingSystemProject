using BankingSystem.Services.Interfaces;
using BankingSystemProject.Core.DTOs;
using BankingSystemProject.Data;
using BankingSystemProject.Data.Tables;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BankingSystemProject.Services.Implementations
{
    public class ATMService : IATMService
    {
        private readonly BankingDbContext context;

        public ATMService(BankingDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// აბრუნებს ბარათზე არსებულ თანხას
        /// </summary>
        public async Task<decimal> GetBalanceAsync(string cardNumber)
        {
            try
            {
                // აბრუნებს ბარათთან დაკავშირებულ საბანკო ანგარიშს
                var bankAccount = await context.Cards
                    .Where(t => t.CardNumber == cardNumber && t.ExpirationDate > DateTime.Now)
                    .Select(t => t.BankAccount)
                    .FirstOrDefaultAsync() ?? throw new BadHttpRequestException("Card not found.");

                // იღებს ბალანსს შესაბამისი ანგარიშიდან
                var balance = bankAccount.Amount;

                return balance;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while getting the balance.");
                throw;
            }
        }

        /// <summary>
        /// ცვლის PIN-ს ბარათზე
        /// </summary>
        public async Task<bool> ChangePinAsync(ChangePinDTO changePinDTO)
        {
            try
            {
                // აბრუნებს ბარათს შეყვანილი ბარათის ნომრის მიხედვით
                var card = await context.Cards
                    .Where(t => t.CardNumber == changePinDTO.CardNumber && t.ExpirationDate > DateTime.Now)
                    .FirstOrDefaultAsync() ?? throw new BadHttpRequestException("Card not found.");

                // აახლებს ძველ პინს ახლით და ინახავს ბაზაში
                card.PIN = changePinDTO.NewPIN;
                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while changing the PIN.");
                throw;
            }
        }

        /// <summary>
        /// უზრუნველყოფს თანხის გამოტანას
        /// </summary>
        public async Task WithdrawMoney(WithdrawMoneyDTO withdrawMoneyDTO)
        {
            try
            {
                var card = await context.Cards
                    .Where(t => t.CardNumber == withdrawMoneyDTO.CardNumber)
                    .FirstOrDefaultAsync();

                if(card == null)
                {
                    Log.Information("Card not found");
                    throw new BadHttpRequestException("Card not found.");
                }

                if (card.ExpirationDate < DateTime.Now)
                {
                    Log.Information("Card is expired");
                    throw new BadHttpRequestException("Card is expired.");
                }
                    
                var account = await context.BankAccounts
                    .Where(a => a.Id == card.BankAccountId)
                    .FirstOrDefaultAsync();

                if(account == null)
                {
                    Log.Information("Account not found");
                    throw new BadHttpRequestException("Account not found.");
                }

                // აბრუნებს ბოლო 24 საათში შესრულებულ ტრანზაქციებს
                var lastTransactions = await context.Withdrawals
                    .Where(t => t.UserId == account.UserId && t.WithdrawalCreateDate <= DateTime.Now && t.WithdrawalCreateDate >= DateTime.Now.AddDays(-1))
                    .ToListAsync();

                decimal lastTransactionsAmount = 0;

                foreach (var lastTransaction in lastTransactions)
                {
                    //თუ ბოლო 24 საათში შესრულებული ტრანზაქცია არ არის ლარში, აკონვერტირებს შესაბამის ვალუტაში
                    if (lastTransaction.CurrencyCode != Core.Enums.Currencies.GEL)
                    {
                        var exchangeRate = await context.ExchangeRates
                            .Where(rate => rate.FromCurrencyCode == lastTransaction.CurrencyCode && rate.ToCurrencyCode == Core.Enums.Currencies.GEL)
                            .Select(t => t.Rate)
                            .FirstOrDefaultAsync();

                        lastTransactionsAmount += (lastTransaction.Amount * exchangeRate);
                    }
                    else
                    {
                        lastTransactionsAmount += lastTransaction.Amount;
                    }

                }

                //ითვლის ბოლო 24 საათში შესრულებული ტრანზაქციები (თანხის განაღდება), ხომ არ აღემატება 10,000 ლარს.
                if (lastTransactionsAmount + withdrawMoneyDTO.Amount >= 10000)
                {
                    Log.Information("The amount withdrawn in 24 hours should not exceed 10,000 GEL");
                    throw new BadHttpRequestException("The amount withdrawn in 24 hours should not exceed 10,000 GEL");
                }
                    

                var withdrawCurrency = account.CurrencyCode;
                decimal commisionRate = 0.02m; //2%
                decimal commisionAmount = withdrawMoneyDTO.Amount * commisionRate;
                var withdrawalAmount = withdrawMoneyDTO.Amount + commisionAmount;

                if (account.CurrencyCode != withdrawMoneyDTO.CurrencyCode)
                {
                    // თუ ანგარიშის ვალუტა და განაღდებისას მითითებული ვალუტა არ ემთხვევა ერთმანეთს საჭიროა კონვერტაცია
                    // აბრუნებს შესაბამის გაცვლით კურსს ვალუტების მიხედვით
                    var exchangeRate = await context.ExchangeRates
                        .Where(rate => rate.ToCurrencyCode == account.CurrencyCode && rate.FromCurrencyCode == withdrawMoneyDTO.CurrencyCode)
                        .Select(t => t.Rate)
                        .FirstOrDefaultAsync();

                    withdrawalAmount = withdrawMoneyDTO.Amount * exchangeRate;

                    // ამოწმებს ანგარიშზე არსებული თანხა არის თუ არა საკმარისი
                    if (account.Amount < withdrawalAmount)
                    {
                        Log.Information("Insufficient balance");
                        throw new BadHttpRequestException("Insufficient balance");
                    }

                    commisionAmount *= exchangeRate;
                    account.Amount -= withdrawalAmount + commisionAmount;
                }
                else
                {
                    if (account.Amount < withdrawalAmount)
                    {
                        Log.Information("Insufficient balance");
                        throw new BadHttpRequestException("Insufficient balance");
                    }

                    account.Amount -= withdrawalAmount;
                }

                // ქმნის ახალ ჩანაწერს ბაზაში, რომელიც გამოიყენება რეპორტინგისთვის
                var withdrawalDTO = new Withdrawal
                {
                    Amount = withdrawMoneyDTO.Amount,
                    CommisionAmount = commisionAmount,
                    CurrencyCode = withdrawMoneyDTO.CurrencyCode,
                    WithdrawalCreateDate = DateTime.Now,
                    UserId = account.UserId
                };

                await context.Withdrawals.AddAsync(withdrawalDTO);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "Error occured while attempting to save data into database");
                throw;
            }catch (Exception ex)
            {
                Log.Error(ex, "Error occured while attempting to use WithdrawMoney method");
                throw;
            }
        }
    }
}