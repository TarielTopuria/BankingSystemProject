using System.Security.Claims;
using BankingSystem.Core.DTOs;
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
    [Authorize(Roles = "Client")] //მეთოდებზე წვდომა აქვს მხოლოდ კლიენტს, ვინაიდან მხოლოდ კლიენტს შეიძლება ჰქონდეს ბარათები და ანგარიშები
    public class NetBankController : ControllerBase
    {
        private readonly INetBankService netBankService;

        public NetBankController(INetBankService netBankService)
        {
            this.netBankService = netBankService;
        }

        /// <summary>
        /// აბრუნებს საბანკო ანგარიშებს იმის მიხედვით თუ ვინაა დალოგინებული
        /// </summary>
        [HttpGet("GetBankAccounts")]
        public async Task<IActionResult> GetBankAccounts()
        {
            try
            {
                // მოაქვს დალოგინებული მომხმარებლის ტოკენში ჩასეტილი ქლაიმები
                var userDataClaim = User.FindFirstValue(ClaimTypes.SerialNumber);

                var bankAccounts = await netBankService.GetBankAccountsForUserAsync(userDataClaim);

                if (bankAccounts == null)
                {
                    Log.Information("Bank Accounts not found");
                    return NotFound("Bank Accounts not found");
                }
                    
                return Ok(bankAccounts);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving bank accounts.");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// აბრუნებს ბარათებს იმის მიხედვით თუ ვინაა დალოგინებული
        /// </summary>
        [HttpGet("GetCards")]
        public async Task<IActionResult> GetCards()
        {
            try
            {
                // მოაქვს დალოგინებული მომხმარებლის ტოკენში ჩასეტილი ქლაიმები
                var userDataClaim = User.FindFirstValue(ClaimTypes.SerialNumber);

                var cards = await netBankService.GetCardsForUserAsync(userDataClaim);

                if (cards == null)
                {
                    Log.Information("Cards not found");
                    return NotFound("Cards not found");
                }

                return Ok(cards);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while retrieving cards.");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// ქმნის ტრანზაქციას დალოგინებული მომხმარებლისთვის
        /// </summary>
        [HttpPost("CreateTransaction")]
        public async Task<IActionResult> CreateTransactionAsync([FromBody] TransactionCreateControllerDTO transactionCreateControllerDTO)
        {
            try
            {
                // მოაქვს დალოგინებული მომხმარებლის ტოკენში ჩასეტილი ქლაიმები
                var userDataClaim = User.FindFirstValue(ClaimTypes.SerialNumber);

                TransactionCreateDTO transactionCreateDTO = new() 
                { 
                    Amount = transactionCreateControllerDTO.Amount, 
                    SenderBankAccountIBAN = transactionCreateControllerDTO.SenderBankAccountIBAN,
                    ReceiverBankAccountIBAN = transactionCreateControllerDTO.ReceiverBankAccountIBAN,
                    CurrencyCode = transactionCreateControllerDTO.CurrencyCode,
                    UserId = userDataClaim
                };

                TransactionCreateValidator validator = new();
                var validation = await validator.ValidateAsync(transactionCreateDTO);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                await netBankService.CreateTransactionAsync(transactionCreateDTO);
                return Ok("Transaction created successfully.");
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex, "An error occurred while creating a transaction: SenderIBAN does not belong to sender user");
                return BadRequest($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating a transaction.");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}