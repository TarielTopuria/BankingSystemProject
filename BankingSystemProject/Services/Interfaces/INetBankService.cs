using BankingSystem.Core.DTOs;
using BankingSystemProject.Data.Tables;

namespace BankingSystem.Services.Interfaces
{
    public interface INetBankService
    {
        Task<List<BankAccount>> GetBankAccountsForUserAsync(string userId);
        Task<List<Card>> GetCardsForUserAsync(string userId);
        Task CreateTransactionAsync(TransactionCreateDTO transactionCreateDTO);
    }
}
