using BankingSystemProject.Core.DTOs;
using BankingSystemProject.Data;
using BankingSystemProject.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Serilog;
using BankingSystemProject.Data.Models;

namespace BankingSystemProject.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> userManager;
        private readonly BankingDbContext context;
        private readonly IConfiguration config;

        public AuthService(UserManager<User> userManager, BankingDbContext context, IConfiguration config)
        {
            this.userManager = userManager;
            this.context = context;
            this.config = config;
        }

        public async Task<string> OnlineBankLogin(LoginUserDTO credentials)
        {
            try
            {
                var identityUser = await userManager.Users.FirstAsync(u => u.Email == credentials.UserName) ?? throw new ArgumentException("User not found");

                if(!await userManager.CheckPasswordAsync(identityUser, credentials.Password))
                {
                    throw new BadHttpRequestException("Username or password is not correct");
                }

                var roleId = context.UserRoles
                    .Where(role => role.UserId == identityUser.Id)
                    .Select(role => role.RoleId)
                    .FirstOrDefault() ?? throw new Exception("Error occured while attempting to get user role Id");

                var roleName = context.Roles
                    .Where(roleName => roleName.Id == roleId)
                    .Select(r => r.Name)
                    .FirstOrDefault() ?? throw new Exception("Error occured while attempting to get user role name");

                var tokenRequest = new TokenGeneratorDTO
                {
                    Credential = identityUser.UserName,
                    Role = roleName,
                    UniqueKey = identityUser.Id
                };

                var tokenString = GenerateTokenString(tokenRequest);

                if(string.IsNullOrEmpty(tokenString))
                {
                    throw new BadHttpRequestException("Error occured while attempting to generate token");
                }

                return tokenString;
            }catch (Exception ex)
            {
                Log.Error(ex, "Error occurred during using OnlineBankLogin method");
                throw;
            }
        }

        public async Task<string> ATMLogin(CardLoginDTO credentials)
        {
            try
            {
                var card = await context.Cards
                    .Where(card => card.CardNumber == credentials.CardNumber && card.PIN == credentials.PIN)
                    .FirstAsync() ?? throw new BadHttpRequestException("CardNumber or PIN is incorrect");

                var bankAccount = await context.BankAccounts
                    .Where(x => x.Id == card.BankAccountId)
                    .Select(x => x.UserId).FirstOrDefaultAsync() ?? throw new BadHttpRequestException("Bank Account not found");

                var user = await context.Users
                    .Where(x => x.Id == bankAccount)
                    .Select(x => x.UserName)
                    .FirstOrDefaultAsync() ?? throw new BadHttpRequestException("User not found");


                var tokenRequest = new TokenGeneratorDTO
                {
                    Credential = user,
                    Role = "CardHolder",
                    UniqueKey = card.CardNumber
                };

                var tokenString = GenerateTokenString(tokenRequest);

                if (string.IsNullOrEmpty(tokenString))
                {
                    throw new BadHttpRequestException("Error occured while attempting to generate token");
                }

                return tokenString;
            }catch (Exception ex)
            {
                Log.Error(ex, "Error occurred during using ATMLogin method");
                throw;
            }
        }

        /// <summary>
        /// Generates a JWT token string based on the provided credentials.
        /// აგენერირებს JWT token ტოკენს, გადაცემული პარამეტრების მიხედვით
        /// </summary>
        /// <param name="credentials">The credentials used to create the token.</param>
        /// <returns>The JWT token string.</returns>
        public string GenerateTokenString(TokenGeneratorDTO credentials)
        {
            try
            {
                // განისაზღვრება ის ქლეიმები, რომლებიც უნდა დაისეტოს ტოკენში, რაც შემდგომ გამოიყენება სხვადასხვა მეთოდების გამოყენების დროს, ქლეიმებიდან ვახდენ საჭირო ინფორმაციის პარამეტრად გადაცემას.
                IEnumerable<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, credentials.Credential), // მეილის ქლეიმი
                    new Claim(ClaimTypes.Role, credentials.Role),         // როლის ქლეიმი, რომელიც გამოიყენება როლზე დაფუძნებული წვდომებისთვის
                    new Claim(ClaimTypes.SerialNumber, credentials.UniqueKey) // SerialNumber-ში ვინახავ UniqueKey-ს, რადგან მარტივად გამოვიყენო ოპერაციებში და პარამეტრი გადავცე ქლაიმებიდან
                };

                // აგენერირებს უსაფრთხოების კოდს JWT secret key-დან რომელიც ისეტება appsettings.json-ში
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetSection("Jwt:Key").Value));

                // security key -სა და HmacSha512Signature ალგორითმის გამოყენებით აგენერირებს ქრედენშენალებს. 
                SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha512Signature);

                // აგენერირებს უშუალოდ ტოკენს, რომელიც შედგება ქლეიმებისგან, ვადისგან, issuer-ის, audience-ის და signing credentials-ებისგან.
                var securityToken = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(60),
                    issuer: config.GetSection("Jwt:Issuer").Value, // სეტავს Issuer-ს, რომელიც მოაქვს appsettings.json-დან
                    audience: config.GetSection("Jwt:Audience").Value, // სეტავს Audience-ს, რომელიც მოაქვს appsettings.json-დან
                    signingCredentials: signingCredentials
                );

                // კასტავს ტოკენს სტრინგში
                var tokenString = new JwtSecurityTokenHandler().WriteToken(securityToken);

                return tokenString;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred during using GenerateTokenString method");
                throw;
            }
        }

    }
}
