using BankingSystemProject.Core.DTOs;

namespace BankingSystemProject.Services.Interfaces
{
    public interface IOperatorService
    {
        Task<bool> RegisterUser(RegisterUserDTO credentials);
        Task AddCardAsync(CardCreateDTO cardCreateDTO);
        Task AddBankAccountAsync(BankAccountCreateDTO bankAccountCreateDTO);
    }
}
