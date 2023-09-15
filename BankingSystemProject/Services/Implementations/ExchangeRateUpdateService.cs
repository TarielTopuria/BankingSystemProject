using BankingSystemProject.Services.Interfaces;
using Serilog;

namespace BankingSystemProject.Services.Implementations
{

    /// <summary>
    /// Background სერვისი, რომელიც უზრუნველყოფს გაცვლითი კურსის დაგეგმილ და ავტომატურ განახლებას.
    /// </summary>
    public class ExchangeRateUpdateService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        public ExchangeRateUpdateService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;

                    // განსაზღვრავს დროს, როდესაც უნდა გაეშვას სერვისი (განსაზღვრულია 17:00, რადგან ეროვნული ბანკი სწორედ ამ დროს ააფდეითებს კურსს)
                    var targetTime = new TimeSpan(17, 00, 00);

                    // ითვლის დროს შემდეგ განახლებამდე
                    var timeUntilNextUpdate = targetTime - now.TimeOfDay;

                    // თუ დრონეგატიურია, სერვისის გაშვება გადაიწევს მეორე დღისთვის
                    if (timeUntilNextUpdate.TotalMilliseconds < 0)
                    {
                        timeUntilNextUpdate = TimeSpan.FromDays(1) + timeUntilNextUpdate;
                    }

                    // ტასკი გადის სლიფზე, მანამ სანამ მისი გაშვების დრო არ მოვა
                    await Task.Delay(timeUntilNextUpdate, stoppingToken);

                    using (var scope = serviceProvider.CreateScope())
                    {
                        var exchangeRateService = scope.ServiceProvider.GetRequiredService<IExchangeRateService>();
                        await exchangeRateService.GetAndUpdateExchangeRatesAsync();
                    }
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred during the exchange rate update.");
                }
            }
        }
    }
}