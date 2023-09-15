using BankingSystemProject.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BankingSystem.Controllers
{
    /// <summary>
    /// კონტროლერი უზრუნველყოფს გაცვლითი კურსის შემოტვირთვას ბაზაში.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin, Manager")]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly IExchangeRateService exchangeRateService;

        /// <summary>
        /// გადატვირთული კონტროლერი, რომელიც უზურნველყოფს Dependency Injection-ის გამოყენებით სერვისების შემოტვირთვას <see cref="ExchangeRatesController"/>.
        /// </summary>
        /// <param name="exchangeRateService"> სერვისი, რომელიც სეტავს გაცვლითი კურსის შემოტვირთვის ლოგიკას </param>
        public ExchangeRatesController(IExchangeRateService exchangeRateService)
        {
            this.exchangeRateService = exchangeRateService;
        }

        /// <summary>
        /// იღებს და ააფდეითებს გაცვლით კურსს გარე API-დან
        /// </summary>
        /// <returns>აბრუნებს ასინქრონულ IActionResult შესაბამისი შეტყობინებით.</returns>
        [HttpGet("GetExchangeRates")]
        public async Task<IActionResult> GetExchangeRatesAsync()
        {
            try
            {
                // GetAndUpdateExchangeRatesAsync მეთოდის გამოყენებით ხდება გაცვლითი კურის შემოტვირთვის მოთხოვნის გაშვება
                string result = await exchangeRateService.GetAndUpdateExchangeRatesAsync();

                // ილოგება წარმატებული შედეგის შესახებ ინფორმაცია 
                Log.Information("Exchange rates updated successfully.");

                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "HTTP request error occurred during exchange rate update.");
                return BadRequest($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred during exchange rate update.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }
    }
}