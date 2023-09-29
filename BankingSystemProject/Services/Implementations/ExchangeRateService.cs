using BankingSystem.Core.DeserializationObjects;
using BankingSystem.Data;
using BankingSystemProject.Core.Enums;
using BankingSystemProject.Data;
using BankingSystemProject.Data.Tables;
using BankingSystemProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;

namespace BankingSystemProject.Services.Implementations
{

    /// <summary>
    /// სერვისის კლასი, რომელიც უზრუნველყოფს გაცვლითი კურსის შემოტვირთვას და დააფდეითებას ბაზაში
    /// </summary>
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient httpClient;
        private readonly string apiURL;
        private readonly BankingDbContext context;

        /// <summary>
        /// ქმნის გადატვირთულ კონსტრუქტორს <see cref="ExchangeRateService"/>.
        /// </summary>
        /// <param name="httpClient">HttpClient ქმნის API რექვესთს.</param>
        /// <param name="apiSettings">API settings უზრუნველყოფს Dependency Injection პრინციპის გამოყენებით რექვსეთის გასაგზავნი ენდპოინტის შემოტვირთვას.</param>
        /// <param name="context">გამოიყენება ბაზასთან სამუშაოდ.</param>
        public ExchangeRateService(HttpClient httpClient, IOptions<ApiSettings> apiSettings, BankingDbContext context)
        {
            this.httpClient = httpClient;
            apiURL = apiSettings.Value.ApiUrl;
            this.context = context;
        }

        /// <summary>
        /// იღებს და ააფდეითებს გაცვლით კურს ექსტერნალ API-ის გამოყენებით
        /// </summary>
        /// <returns>აბრუნებს შეტყობინებას გაცვლითი კურსის შესახებ</returns>
        public async Task<string> GetAndUpdateExchangeRatesAsync()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(apiURL);

                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = await response.Content.ReadAsStringAsync();

                    //ახდენს მიღებული JSON -ის დესერიალიზაციას დაკონკრეტებულ კლასში.
                    List<ExchangeRateData> jsonData = JsonConvert.DeserializeObject<List<ExchangeRateData>>(jsonContent);

                    var exchangeRateData = jsonData.FirstOrDefault();

                    if (exchangeRateData != null)
                    {
                        // იღებს კურსს GEL-დან USD-ში და GEL-დან EUR-ში
                        decimal gelToUsdRate = exchangeRateData.currencies
                            .FirstOrDefault(currency => currency.code == "USD")?.rate ?? 0;

                        decimal gelToEurRate = exchangeRateData.currencies
                            .FirstOrDefault(currency => currency.code == "EUR")?.rate ?? 0;

                        // ითვლის სხვადასხვა კომბინაციას გაცვლითი კურსისთვის
                        await UpdateExchangeRate(CurrenciesEnum.GEL, CurrenciesEnum.USD, 1 / gelToUsdRate);
                        await UpdateExchangeRate(CurrenciesEnum.GEL, CurrenciesEnum.EUR, 1 / gelToEurRate);
                        await UpdateExchangeRate(CurrenciesEnum.USD, CurrenciesEnum.GEL, gelToUsdRate);
                        await UpdateExchangeRate(CurrenciesEnum.EUR, CurrenciesEnum.GEL, gelToEurRate);
                        await UpdateExchangeRate(CurrenciesEnum.USD, CurrenciesEnum.EUR, gelToUsdRate / gelToEurRate);
                        await UpdateExchangeRate(CurrenciesEnum.EUR, CurrenciesEnum.USD, gelToEurRate / gelToUsdRate);

                        return "Exchange rates updated successfully.";
                    }
                    else
                    {
                        return "Exchange rate data not found in the response.";
                    }
                }
                else
                {
                    // ლოგავს დაბრუნებულ სტატუს კოდს
                    Log.Error($"Error: {response.StatusCode}");
                    return $"Error: {response.StatusCode}";
                }
            }
            catch (HttpRequestException ex)
            {
                // ლოგავს HTTP ერორს
                Log.Error(ex, "HTTP request error occurred during exchange rate update.");
                return $"HTTP request error: {ex.Message}";
            }
            catch (Exception ex)
            {
                // ლოგავს სხვა გაუთვალისწინებელ შეცდომებს
                Log.Error(ex, "An unexpected error occurred during exchange rate update.");
                return "An unexpected error occurred.";
            }
        }

        /// <summary>
        /// დამხმარე მეთოდი, რომელიც ააფდეითებს გაცვლით კურსს ბაზაში
        /// </summary>
        /// <param name="fromCurrencyCode">გაცვლითი კურსი -დან.</param>
        /// <param name="toCurrencyCode">გაცვლითი კურსი -ში.</param>
        /// <param name="rate">გაცვლითი კურსის მნიშვნელობა.</param>
        private async Task UpdateExchangeRate(CurrenciesEnum fromCurrencyCode, CurrenciesEnum toCurrencyCode, decimal rate)
        {
            try
            {
                // ამოწმებს გაცვლითი კურსი არსებობს თუ არა უკვე ბაზაში
                var existingRate = await context.ExchangeRates
                    .Where(r => r.FromCurrencyCode == fromCurrencyCode && r.ToCurrencyCode == toCurrencyCode)
                    .FirstOrDefaultAsync();

                if (existingRate != null)
                {
                    // თუ არსებობს გადააწერს ახალ მნიშვნელობას
                    existingRate.Rate = rate;
                    existingRate.LastUpdated = DateTime.Now;
                }
                else
                {
                    // თუ არ არსებობს შექმნის ახალ მნიშვნელობას
                    var newRate = new ExchangeRate
                    {
                        FromCurrencyCode = fromCurrencyCode,
                        ToCurrencyCode = toCurrencyCode,
                        Rate = rate,
                        LastUpdated = DateTime.Now
                    };
                    await context.ExchangeRates.AddAsync(newRate);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while updating exchange rate in the database.");
            }
        }
    }
}