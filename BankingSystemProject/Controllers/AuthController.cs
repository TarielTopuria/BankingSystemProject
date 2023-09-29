using BankingSystemProject.Core.DTOs;
using BankingSystemProject.Core.Validators;
using BankingSystemProject.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BankingSystemProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // კონტროლერზე წვდომა გახსნილია არააუთენთიფიცირებული მომმხარებლებისთვის, რადგან მეთოდები ტოკენების გენერირებისთვის გამოიყენება
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost("OnlineBankLogin")]
        public async Task<IActionResult> OnlineBankLogin(LoginUserDTO credentials)
        {
            try
            {
                LoginUserValidator validator = new();
                var validation = await validator.ValidateAsync(credentials);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                var token = await authService.OnlineBankLogin(credentials);
                return Ok(token);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex, "Username or password is incorrect");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred during OnlineBankLogin");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("ATMLogin")]
        public async Task<IActionResult> ATMLogin(CardLoginDTO credentials)
        {
            try
            {
                CardLoginValidator validator = new();
                var validation = await validator.ValidateAsync(credentials);

                if (!validation.IsValid)
                {
                    return BadRequest(validation.Errors);
                }

                var token = await authService.ATMLogin(credentials);
                return Ok(token);
            }
            catch (BadHttpRequestException ex)
            {
                Log.Error(ex.Message, "Error occurred during ATMLogin");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unexpected error occurred during ATMLogin");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
