using BankingSystemProject.Core.DTOs;

namespace BankingSystemProject.Services.Interfaces
{
    public interface IAuthService
    {
        string GenerateTokenString(TokenGeneratorDTO credentials);
        Task<string> OnlineBankLogin(LoginUserDTO credentials);
        Task<string> ATMLogin(CardLoginDTO credentials);
    }
}
