using BankingSystem.Core.DTOs;
using BankingSystemProject.Core.DTOs;
using BankingSystemProject.Data.Tables;

namespace BankingSystem.Services.Interfaces
{
    public interface INetBankService
    {
        Task<List<BankAccountResponseDTO>> GetBankAccountsForUserAsync(string userId);
        Task<List<CardResponseDTO>> GetCardsForUserAsync(string userId);
        Task CreateTransactionAsync(TransactionCreateDTO transactionCreateDTO);
    }
}
