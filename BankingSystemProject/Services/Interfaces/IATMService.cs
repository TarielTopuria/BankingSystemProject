using BankingSystemProject.Core.DTOs;

namespace BankingSystem.Services.Interfaces
{
    public interface IATMService
    {
        Task<decimal> GetBalanceAsync(string cardNumber);
        Task<bool> ChangePinAsync(ChangePinDTO changePinDTO);
        Task WithdrawMoney(WithdrawMoneyDTO withdrawMoneyDTO);
    }
}
