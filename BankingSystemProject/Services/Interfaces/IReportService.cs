using BankingSystem.Core.DTOs;

namespace BankingSystem.Services.Interfaces
{
    public interface IReportService
    {
        Task<int> GetClientRegistrationCountAsync(BetweenDateDTO dates);
        Task<int> GetTransactionsCountAsync(BetweenDateDTO dates);
        Task<List<KeyValuePair<DateTime, int>>> GetTransactionsCountByDayAsync(BetweenDateDTO dates);
        Task<decimal> GetTransactionsAmountAsync(BetweenDateCurrencyDTO value);
        Task<decimal> GetTransactionsAmountMeanAsync(BetweenDateCurrencyDTO value);
        Task<decimal> GetCommisionAmountFromTransactionsAsync(BetweenDateCurrencyDTO value);
        Task<decimal> GetCommisionAmountMeanFromTransactionsAsync(BetweenDateCurrencyDTO value);
        Task<int> GetWithdrawalsCountAsync(BetweenDateDTO dates);
        Task<List<KeyValuePair<DateTime, int>>> GetWithdrawalsCountByDayAsync(BetweenDateDTO dates);
        Task<decimal> GetWithdrawalsAmountAsync(BetweenDateCurrencyDTO value);
        Task<decimal> GetWithdrawalsAmountMeanAsync(BetweenDateCurrencyDTO value);
        Task<decimal> GetCommisionsAmountFromWithdrawalsAsync(BetweenDateCurrencyDTO value);
        Task<decimal> GetCommisionsAmountMeanFromWithdrawalsAsync(BetweenDateCurrencyDTO value);
    }
}
