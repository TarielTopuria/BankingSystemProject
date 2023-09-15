using BankingSystem.Core.DTOs;
using BankingSystem.Services.Interfaces;
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

        [HttpPost("RegisteredClientCount")]
        public async Task<IActionResult> GetRegisteredClientCount([FromBody] BetweenDateDTO dates)
        {
            try
            {
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

        [HttpPost("TransactionsCount")]
        public async Task<IActionResult> GetTransactionsCount([FromBody] BetweenDateDTO dates)
        {
            try
            {
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

        [HttpPost("TransactionsCountChart")]
        public async Task<IActionResult> GetTransactionsCountChart([FromBody] BetweenDateDTO dates)
        {
            try
            {
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

        [HttpPost("WithdrawalsCountChart")]
        public async Task<IActionResult> GetWithdrawalsCountByDay([FromBody] BetweenDateDTO dates)
        {
            try
            {
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

        [HttpPost("GetWithdrawalsCount")]
        public async Task<IActionResult> GetWithdrawalsCount([FromBody] BetweenDateDTO dates)
        {
            try
            {
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

        [HttpPost("TransactionsAmount")]
        public async Task<IActionResult> GetTransactionsAmount([FromBody] BetweenDateCurrencyDTO value)
        {
            try
            {
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

        [HttpPost("WithdrawalsAmount")]
        public async Task<IActionResult> GetWithdrawalsAmount([FromBody] BetweenDateCurrencyDTO value)
        {
            try
            {
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

        [HttpPost("TransactionsAmountMean")]
        public async Task<IActionResult> GetTransactionsAmountMean([FromBody] BetweenDateCurrencyDTO value)
        {
            try
            {
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

        [HttpPost("WithdrawalsAmountMean")]
        public async Task<IActionResult> GetWithdrawalsAmountMean([FromBody] BetweenDateCurrencyDTO value)
        {
            try
            {
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

        [HttpPost("CommisionAmountFromTransactions")]
        public async Task<IActionResult> GetCommisionsAmountFromTransactions([FromBody] BetweenDateCurrencyDTO value)
        {
            try
            {
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

        [HttpPost("CommisionsAmountFromWithdrawals")]
        public async Task<IActionResult> GetCommisionsAmountFromWithdrawals([FromBody] BetweenDateCurrencyDTO value)
        {
            try
            {
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

        [HttpPost("CommisionAmountMeanFromTransactions")]
        public async Task<IActionResult> GetCommisionAmountMeanFromTransactions([FromBody] BetweenDateCurrencyDTO value)
        {
            try
            {
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

        [HttpPost("CommisionsAmountMeanFromWithdrawals")]
        public async Task<IActionResult> GetCommisionsAmountMeanFromWithdrawals([FromBody] BetweenDateCurrencyDTO value)
        {
            try
            {
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
