namespace BankingSystemProject.Services.Interfaces
{
    public interface IExchangeRateService
    {
        Task<string> GetAndUpdateExchangeRatesAsync();
    }
}
