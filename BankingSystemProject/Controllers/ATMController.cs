using System.Security.Claims;
using BankingSystem.Services.Interfaces;
using BankingSystemProject.Core.DTOs;
using BankingSystemProject.Core.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BankingSystemProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "CardHolder")]
    public class ATMController : ControllerBase
    {
        private readonly IATMService atmService;

        public ATMController(IATMService atmService)
        {
            this.atmService = atmService;
        }

        /// <summary>
        /// აბრუნებს ბალანს ავტორიზირებული მომხმარებლისთვის
        /// </summary>
        [HttpPost("GetBalance")]
        public async Task<IActionResult> GetBalanceAsync()
        {
            try
            {
                var userDataClaim = User.FindFirstValue(ClaimTypes.SerialNumber);
                if (userDataClaim == null)
                {
                    Log.Information("UserData claim not found.");
                    return BadRequest("UserData claim not found.");
                }

                var balance = await atmService.GetBalanceAsync(userDataClaim);
                return Ok(balance);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error($"An error occurred: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// ცვლის პინს ავტორიზირებული მომხმარებლისთვის
        /// </summary>
        [HttpPatch("ChangePin")]
        public async Task<IActionResult> ChangePinAsync([FromBody] string newPIN)
        {
            try
            {
                var userDataClaim = User.FindFirstValue(ClaimTypes.SerialNumber);
                if (userDataClaim == null)
                {
                    Log.Information("UserData claim not found.");
                    return BadRequest("UserData claim not found.");
                }

                if (string.IsNullOrEmpty(newPIN) || newPIN.Length != 4)
                {
                    Log.Information("Invalid PIN format. The PIN must be a 4-digit code.");
                    return BadRequest("Invalid PIN format. The PIN must be a 4-digit code.");
                }

                var pinToChange = new ChangePinDTO()
                {
                    CardNumber = userDataClaim,
                    NewPIN = newPIN,
                };

                await atmService.ChangePinAsync(pinToChange);

                return Ok("PIN code changed successfully.");
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
                Log.Error($"An error occurred: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// გამოაქვს თანხა ავტორიზირებული მომხმარებლის ანგარიშიდან
        /// </summary>
        [HttpPost("Withdrawal")]
        public async Task<IActionResult> Withdrawal(WithdrawMoneyControllerDTO withdrawMoneyControllerDTO)
        {
            try
            {
                WithdrawMoneyValidator validator = new();
                var validation = await validator.ValidateAsync(withdrawMoneyControllerDTO);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                var userDataClaim = User.FindFirstValue(ClaimTypes.SerialNumber);
                if (userDataClaim == null)
                {
                    Log.Information("UserData claim not found.");
                    return BadRequest("UserData claim not found.");
                }

                var withdrawal = new WithdrawMoneyDTO()
                {
                    CardNumber = userDataClaim,
                    Amount = withdrawMoneyControllerDTO.Amount,
                    CurrencyCode = withdrawMoneyControllerDTO.CurrencyCode
                };

                await atmService.WithdrawMoney(withdrawal);
                return Ok($"{withdrawMoneyControllerDTO.Amount} withdrawn from account");
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error($"An error occurred: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error($"An error occurred: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}