using BankingSystem.Core.DTOs;
using BankingSystem.Services.Interfaces;
using BankingSystemProject.Core.Enums;
using BankingSystemProject.Core.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BankingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin, Manager")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService reportService;
        public ReportController(IReportService reportService)
        {
            this.reportService = reportService;
        }

        [HttpGet("RegisteredClientCount")]
        public async Task<IActionResult> GetRegisteredClientCount(DateTime fromDate, DateTime toDate)
        {
            try
            {
                BetweenDateDTO dates = new() { FromDate = fromDate, ToDate = toDate };
                BetweenDateValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(dates);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                return Ok(await reportService.GetClientRegistrationCountAsync(dates));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("TransactionsCount")]
        public async Task<IActionResult> GetTransactionsCount(DateTime fromDate, DateTime toDate)
        {
            try
            {
                BetweenDateDTO dates = new() { FromDate = fromDate, ToDate = toDate };
                BetweenDateValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(dates);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                return Ok(await reportService.GetTransactionsCountAsync(dates));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("TransactionsCountChart")]
        public async Task<IActionResult> GetTransactionsCountChart(DateTime fromDate, DateTime toDate)
        {
            try
            {
                BetweenDateDTO dates = new() { FromDate = fromDate, ToDate = toDate };
                BetweenDateValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(dates);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                return Ok(await reportService.GetTransactionsCountByDayAsync(dates));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("WithdrawalsCountChart")]
        public async Task<IActionResult> GetWithdrawalsCountByDay(DateTime fromDate, DateTime toDate)
        {
            try
            {
                BetweenDateDTO dates = new() { FromDate = fromDate, ToDate = toDate };
                BetweenDateValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(dates);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                return Ok(await reportService.GetWithdrawalsCountByDayAsync(dates));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetWithdrawalsCount")]
        public async Task<IActionResult> GetWithdrawalsCount(DateTime fromDate, DateTime toDate)
        {
            try
            {
                BetweenDateDTO dates = new() { FromDate = fromDate, ToDate = toDate };
                BetweenDateValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(dates);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                return Ok(await reportService.GetWithdrawalsCountAsync(dates));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("TransactionsAmount")]
        public async Task<IActionResult> GetTransactionsAmount(DateTime fromDate, DateTime toDate, CurrenciesEnum currency)
        {
            try
            {
                BetweenDateCurrencyDTO value = new() { FromDate = fromDate, ToDate = toDate, CurrencyCode = currency };
                BetweenDateCurrencyValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(value);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                return Ok(await reportService.GetTransactionsAmountAsync(value));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("WithdrawalsAmount")]
        public async Task<IActionResult> GetWithdrawalsAmount(DateTime fromDate, DateTime toDate, CurrenciesEnum currency)
        {
            try
            {
                BetweenDateCurrencyDTO value = new() { FromDate = fromDate, ToDate = toDate, CurrencyCode = currency };
                BetweenDateCurrencyValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(value);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                return Ok(await reportService.GetWithdrawalsAmountAsync(value));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("TransactionsAmountMean")]
        public async Task<IActionResult> GetTransactionsAmountMean(DateTime fromDate, DateTime toDate, CurrenciesEnum currency)
        {
            try
            {
                BetweenDateCurrencyDTO value = new() { FromDate = fromDate, ToDate = toDate, CurrencyCode = currency };
                BetweenDateCurrencyValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(value);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                return Ok(await reportService.GetTransactionsAmountMeanAsync(value));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("WithdrawalsAmountMean")]
        public async Task<IActionResult> GetWithdrawalsAmountMean(DateTime fromDate, DateTime toDate, CurrenciesEnum currency)
        {
            try
            {
                BetweenDateCurrencyDTO value = new() { FromDate = fromDate, ToDate = toDate, CurrencyCode = currency };
                BetweenDateCurrencyValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(value);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                return Ok(await reportService.GetWithdrawalsAmountMeanAsync(value));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("CommisionAmountFromTransactions")]
        public async Task<IActionResult> GetCommisionsAmountFromTransactions(DateTime fromDate, DateTime toDate, CurrenciesEnum currency)
        {
            try
            {
                BetweenDateCurrencyDTO value = new() { FromDate = fromDate, ToDate = toDate, CurrencyCode = currency };
                BetweenDateCurrencyValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(value);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                return Ok(await reportService.GetCommisionAmountFromTransactionsAsync(value));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("CommisionsAmountFromWithdrawals")]
        public async Task<IActionResult> GetCommisionsAmountFromWithdrawals(DateTime fromDate, DateTime toDate, CurrenciesEnum currency)
        {
            try
            {
                BetweenDateCurrencyDTO value = new() { FromDate = fromDate, ToDate = toDate, CurrencyCode = currency };
                BetweenDateCurrencyValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(value);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                return Ok(await reportService.GetCommisionsAmountFromWithdrawalsAsync(value));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("CommisionAmountMeanFromTransactions")]
        public async Task<IActionResult> GetCommisionAmountMeanFromTransactions(DateTime fromDate, DateTime toDate, CurrenciesEnum currency)
        {
            try
            {
                BetweenDateCurrencyDTO value = new() { FromDate = fromDate, ToDate = toDate, CurrencyCode = currency };
                BetweenDateCurrencyValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(value);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                return Ok(await reportService.GetCommisionAmountMeanFromTransactionsAsync(value));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("CommisionsAmountMeanFromWithdrawals")]
        public async Task<IActionResult> GetCommisionsAmountMeanFromWithdrawals(DateTime fromDate, DateTime toDate, CurrenciesEnum currency)
        {
            try
            {
                BetweenDateCurrencyDTO value = new() { FromDate = fromDate, ToDate = toDate, CurrencyCode = currency };
                BetweenDateCurrencyValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(value);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                return Ok(await reportService.GetCommisionsAmountMeanFromWithdrawalsAsync(value));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
