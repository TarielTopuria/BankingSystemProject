using BankingSystem.Core.DTOs;
using BankingSystem.Services.Interfaces;
using BankingSystemProject.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BankingSystem.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly BankingDbContext context;
        public ReportService(BankingDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// აგენერირებს რეპორტს თუ რამდენი მომხმარებელი დარეგისტრირდა მითითებული დროის შუალედში
        /// </summary>
        public async Task<int> GetClientRegistrationCountAsync(BetweenDateDTO dates)
        {
            try
            {
                var result = await context.Users
                    .Where(u => u.RegistrationDate >= dates.FromDate && u.RegistrationDate <= dates.ToDate)
                    .CountAsync();
                return result;
            }catch (Exception ex) 
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message); 
            }
        }

        /// <summary>
        /// აგენერირებს რეპორტს თუ რა ოდენობის საკომისიო (სარგებელი) მიიღო ბანკმა მითითებული დროის შუალედში და მითითებულ ვალუტაში
        /// </summary>
        public async Task<decimal> GetCommisionAmountFromTransactionsAsync(BetweenDateCurrencyDTO value)
        {
            try
            {
                var transactions = await context.Transactions
                    .Where(u => u.TransactionCreateDate >= value.FromDate && u.TransactionCreateDate <= value.ToDate && u.CurrencyCode == value.CurrencyCode)
                    .ToListAsync() ?? throw new BadHttpRequestException("Transactions not found");

                var transactionsAmount = 0m;

                foreach (var transaction in transactions)
                {
                    transactionsAmount += transaction.CommisionAmount;
                }

                return transactionsAmount;

            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// აგენერირებს რეპორტს თუ საშუალოდ რა ოდენობის საკომისიო (სარგებელი) მიიღო ბანკმა მითითებული დროის შუალედში და მითითებულ ვალუტაში
        /// </summary>
        public async Task<decimal> GetCommisionAmountMeanFromTransactionsAsync(BetweenDateCurrencyDTO value)
        {
            try
            {
                var transactions = await context.Transactions
                    .Where(u => u.TransactionCreateDate >= value.FromDate && u.TransactionCreateDate <= value.ToDate && u.CurrencyCode == value.CurrencyCode)
                    .ToListAsync() ?? throw new BadHttpRequestException("Transactions not found");

                var transactionsAmount = 0m;

                foreach (var transaction in transactions)
                {
                    transactionsAmount += transaction.CommisionAmount;
                }
                return transactionsAmount / transactions.Count;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// აგენერირებს რეპორტს თუ რა ოდენობის გადარიცხვა (ტრანზაქცია) განხორციელდა მითითებული დროის შუალედში და მითითებულ ვალუტაში
        /// </summary>
        public async Task<decimal> GetTransactionsAmountAsync(BetweenDateCurrencyDTO value)
        {
            try
            {
                var transactions = await context.Transactions
                    .Where(u => u.TransactionCreateDate >= value.FromDate && u.TransactionCreateDate <= value.ToDate && u.CurrencyCode == value.CurrencyCode)
                    .ToListAsync() ?? throw new BadHttpRequestException("Transactions not found");

                var transactionsAmount = 0m;

                foreach (var transaction in transactions)
                {
                    transactionsAmount += (transaction.Amount + transaction.CommisionAmount);
                }
                return transactionsAmount;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// აგენერირებს რეპორტს თუ საშუალოდ რა ოდენობის გადარიცხვა (ტრანზაქცია) განხორციელდა მითითებული დროის შუალედში და მითითებულ ვალუტაში
        /// </summary>
        public async Task<decimal> GetTransactionsAmountMeanAsync(BetweenDateCurrencyDTO value)
        {
            try
            {
                var transactions = await context.Transactions
                    .Where(u => u.TransactionCreateDate >= value.FromDate && u.TransactionCreateDate <= value.ToDate && u.CurrencyCode == value.CurrencyCode)
                    .ToListAsync() ?? throw new BadHttpRequestException("Transactions not found");

                var transactionsAmount = 0m;

                foreach (var transaction in transactions)
                {
                    transactionsAmount += (transaction.Amount + transaction.CommisionAmount);
                }
                return transactionsAmount / transactions.Count;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// აგენერირებს რეპორტს თუ რამდენი გადარიცხვა (ტრანზაქცია) განხორციელდა მითითებული დროის შუალედში
        /// </summary>
        public async Task<int> GetTransactionsCountAsync(BetweenDateDTO dates)
        {
            try
            {
                var result = await context.Transactions
                    .Where(u => u.TransactionCreateDate >= dates.FromDate && u.TransactionCreateDate <= dates.ToDate)
                    .CountAsync();

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// აგენერირებს რეპორტს თუ რამდენი გადარიცხვა (ტრანზაქცია) განხორციელდა მითითებული დროის შუალედში, დღეების ჭრილში
        /// </summary>
        public async Task<List<KeyValuePair<DateTime, int>>> GetTransactionsCountByDayAsync(BetweenDateDTO dates)
        {
            try
            {
                // Filter transactions within the last month
                var result = await context.Transactions
                    .Where(u => u.TransactionCreateDate >= dates.FromDate && u.TransactionCreateDate <= dates.ToDate)
                    .GroupBy(u => u.TransactionCreateDate.Date) // Group by the date part (day) of TransactionCreateDate
                    .OrderBy(g => g.Key) // Order by date
                    .Select(g => new KeyValuePair<DateTime, int>(g.Key, g.Count())) // Create key-value pairs
                    .ToListAsync() ?? throw new Exception("Error occured while counting transactions");

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// აგენერირებს რეპორტს თუ ATM-დან რამდენჯერ განხორციელდა თანხის გატანა, მითითებული დროის შუალედში
        /// </summary>
        public async Task<int> GetWithdrawalsCountAsync(BetweenDateDTO dates)
        {
            try
            {
                var result = await context.Withdrawals
                    .Where(u => u.WithdrawalCreateDate >= dates.FromDate && u.WithdrawalCreateDate <= dates.ToDate)
                    .CountAsync();

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// აგენერირებს რეპორტს თუ ATM-დან რამდენჯერ განხორციელდა თანხის გატანა, მითითებული დროის შუალედში, დღეების მიხედვით (ჩარტი)
        /// </summary>
        public async Task<List<KeyValuePair<DateTime, int>>> GetWithdrawalsCountByDayAsync(BetweenDateDTO dates)
        {
            try
            {
                // Filter transactions within the last month
                var result = await context.Withdrawals
                    .Where(u => u.WithdrawalCreateDate >= dates.FromDate && u.WithdrawalCreateDate <= dates.ToDate)
                    .GroupBy(u => u.WithdrawalCreateDate.Date) // Group by the date part (day) of TransactionCreateDate
                    .OrderBy(g => g.Key) // Order by date
                    .Select(g => new KeyValuePair<DateTime, int>(g.Key, g.Count())) // Create key-value pairs
                    .ToListAsync() ?? throw new Exception("Error occured while counting transactions");

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// აგენერირებს რეპორტს თუ ATM-დან რა ოდენობის თანხის განახდება განხორციელდა, მითითებული დროის შუალედში, რაც კონვერტირდება ვალუტაში
        /// </summary>
        public async Task<decimal> GetWithdrawalsAmountAsync(BetweenDateCurrencyDTO value)
        {
            try
            {
                var withdrawals = await context.Withdrawals
                    .Where(u => u.WithdrawalCreateDate >= value.FromDate && u.WithdrawalCreateDate <= value.ToDate && u.CurrencyCode == value.CurrencyCode)
                    .ToListAsync() ?? throw new BadHttpRequestException("Withdrawals not found");

                var withdrawalsAmount = 0m;

                foreach (var withdrawal in withdrawals)
                {
                    withdrawalsAmount += (withdrawal.Amount + withdrawal.CommisionAmount);
                }
                return withdrawalsAmount;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// აგენერირებს რეპორტს თუ საშუალოდ ATM-დან რა ოდენობის თანხის განახდება განხორციელდა, მითითებული დროის შუალედში, რაც კონვერტირდება ვალუტაში
        /// </summary>
        public async Task<decimal> GetWithdrawalsAmountMeanAsync(BetweenDateCurrencyDTO value)
        {
            try
            {
                var withdrawals = await context.Withdrawals
                    .Where(u => u.WithdrawalCreateDate >= value.FromDate && u.WithdrawalCreateDate <= value.ToDate && u.CurrencyCode == value.CurrencyCode)
                    .ToListAsync() ?? throw new BadHttpRequestException("Withdrawals not found");

                var withdrawalsAmount = 0m;

                foreach (var withdrawal in withdrawals)
                {
                    withdrawalsAmount += (withdrawal.Amount + withdrawal.CommisionAmount);
                }
                return withdrawalsAmount / withdrawals.Count;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// აგენერირებს რეპორტს თუ ATM-დან რა ოდენობის სარგებლის (საკომისიოს) მიღება განხორციელდა, მითითებული დროის შუალედში, რაც კონვერტირდება ვალუტაში
        /// </summary>
        public async Task<decimal> GetCommisionsAmountFromWithdrawalsAsync(BetweenDateCurrencyDTO value)
        {
            try
            {
                var withdrawals = await context.Withdrawals
                    .Where(u => u.WithdrawalCreateDate >= value.FromDate && u.WithdrawalCreateDate <= value.ToDate && u.CurrencyCode == value.CurrencyCode)
                    .ToListAsync() ?? throw new BadHttpRequestException("Withdrawals not found");

                var withdrawalsAmount = 0m;

                foreach (var withdrawal in withdrawals)
                {
                    withdrawalsAmount += withdrawal.CommisionAmount;
                }

                return withdrawalsAmount;

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// აგენერირებს რეპორტს თუ ATM-დან საშუალოდ რა ოდენობის სარგებლის (საკომისიოს) მიღება განხორციელდა, მითითებული დროის შუალედში, რაც კონვერტირდება ვალუტაში
        /// </summary>
        public async Task<decimal> GetCommisionsAmountMeanFromWithdrawalsAsync(BetweenDateCurrencyDTO value)
        {
            try
            {
                var withdrawals = await context.Withdrawals
                    .Where(u => u.WithdrawalCreateDate >= value.FromDate && u.WithdrawalCreateDate <= value.ToDate && u.CurrencyCode == value.CurrencyCode)
                    .ToListAsync() ?? throw new BadHttpRequestException("Withdrawals not found");

                var withdrawalsAmount = 0m;

                foreach (var withdrawal in withdrawals)
                {
                    withdrawalsAmount += withdrawal.CommisionAmount;
                }
                return withdrawalsAmount / withdrawals.Count;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}
