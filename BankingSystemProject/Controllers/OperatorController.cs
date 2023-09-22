using BankingSystem.Core.DTOs;
using BankingSystemProject.Core.DTOs;
using BankingSystemProject.Core.Validators;
using BankingSystemProject.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BankingSystemProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // კონტროლერში შემავალი მეთოდები დაშვებულია ორ როლზე, ოპერატორსა და ადმინზე
    [Authorize(Roles = "Operator, Admin")]
    public class OperatorController : ControllerBase
    {
        private readonly IOperatorService operatorService;

        public OperatorController(IOperatorService operatorService)
        {
            this.operatorService = operatorService;
        }

        [HttpPost("RegisterUser")]
        [AllowAnonymous] // დაშვებულია ტესტირების მიზნებისთივს
        public async Task<IActionResult> RegisterUser(RegisterUserDTO credentials)
        {
            try
            {
                RegisterUserValidator validator = new();
                var validation = await validator.ValidateAsync(credentials);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                if (await operatorService.RegisterUser(credentials))
                {
                    return Ok("User created successfully");
                }

                Log.Error("BadRequest Error occured while attempting to Register User");
                return BadRequest();
            }
            catch (Exception ex)
            {
                // ექსეფშენის დალოგვა
                Log.Error(ex, "An error occurred while registering a user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
        }

        [HttpPost("CreateBankAccount")]
        public async Task<IActionResult> CreateBankAccount(BankAccountCreateDTO bankAccountToCreate)
        {
            try
            {
                BankAccountCreateValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(bankAccountToCreate);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                await operatorService.AddBankAccountAsync(bankAccountToCreate);
                return Ok("Bank Account created successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating a bank account.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
        }

        [HttpPost("CreateCard")]
        public async Task<IActionResult> CreateCard(CardCreateDTO cardToCreate)
        {
            try
            {
                CardCreateValidator validatorObj = new();
                var validation = await validatorObj.ValidateAsync(cardToCreate);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                await operatorService.AddCardAsync(cardToCreate);
                return Ok("Card created successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while creating a card.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the request.");
            }
        }
    }
}
