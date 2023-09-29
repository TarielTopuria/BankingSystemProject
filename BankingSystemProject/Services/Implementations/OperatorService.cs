using AutoMapper;
using BankingSystemProject.Core.DTOs;
using BankingSystemProject.Core.Enums;
using BankingSystemProject.Data;
using BankingSystemProject.Data.Models;
using BankingSystemProject.Data.Tables;
using BankingSystemProject.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using Serilog;

namespace BankingSystemProject.Services.Implementations
{
    public class OperatorService : IOperatorService
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly BankingDbContext context;
        private readonly IMapper mapper;

        public OperatorService(UserManager<User> userManager, BankingDbContext context, IMapper mapper, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.context = context;
            this.mapper = mapper;
            this.roleManager = roleManager;
        }

        public async Task<bool> RegisterUser(RegisterUserDTO credentials)
        {
            try
            {
                var identityUser = new User
                {
                    FirstName = credentials.FirstName,
                    LastName = credentials.LastName,
                    PersonalNumber = credentials.PersonalNumber,
                    DateOfBirth = credentials.DateOfBirth,
                    UserName = credentials.Email,
                    Email = credentials.Email,
                    RegistrationDate = DateTime.Now
                };

                var userExist = await context.Users.AnyAsync(x => x.PersonalNumber == credentials.PersonalNumber);

                if (userExist)
                {
                    throw new BadHttpRequestException("User with this personal number already exists");
                }

                var emailExist = await context.Users.AnyAsync(x => x.Email == credentials.Email);

                if (emailExist)
                {
                    throw new BadHttpRequestException("Email already exists");
                }

                var result = await userManager.CreateAsync(identityUser, credentials.Password);

                // ენამიდან გამომდინარე ინახავს სტრინგად როლის დასახელებას
                var roleName = Enum.GetName(typeof(RolesEnum), credentials.Role);

                Console.WriteLine(roleName);

                try
                {
                    await userManager.AddToRoleAsync(identityUser, roleName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, "An error occurred while adding role to the user.");
                    throw new Exception($"An error occurred while adding role to the user: {ex.Message}");
                }

                var roles = await roleManager.Roles.ToListAsync();
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "An error occurred while registering a user.");
                throw; // ხელახლა ვასროლინებ ექსეფშენს დეტალური ინფორმაციისთვის
            }
        }


        public async Task AddBankAccountAsync(BankAccountCreateDTO bankAccountCreateDTO)
        {
            try
            {
                var user = await userManager.FindByIdAsync(bankAccountCreateDTO.UserId);

                if (user == null)
                {
                    Log.Error($"User with ID {bankAccountCreateDTO.UserId} not found.");
                    throw new BadHttpRequestException($"User with ID {bankAccountCreateDTO.UserId} not found.");
                }

                if (!await userManager.IsInRoleAsync(user, Enum.GetName(RolesEnum.Client)))
                {
                    Log.Error($"User {user.UserName} does not have the {Enum.GetName(RolesEnum.Client)} role.");
                    throw new InvalidOperationException($"User {bankAccountCreateDTO.UserId} must have the {Enum.GetName(RolesEnum.Client)} role to have a bank account.");
                }

                var accountExist = await context.BankAccounts.AnyAsync(x => x.IBAN == bankAccountCreateDTO.IBAN);
                if (accountExist)
                {
                    Log.Error($"Bank account with this IBAN {bankAccountCreateDTO.IBAN} already exists");
                    throw new BadHttpRequestException($"Bank account with this IBAN {bankAccountCreateDTO.IBAN} already exists");
                }
                var bankAccountToCreate = mapper.Map<BankAccount>(bankAccountCreateDTO);
                await context.BankAccounts.AddAsync(bankAccountToCreate);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "An error occurred while adding a bank account.");
                throw;
            }
        }

        public async Task AddCardAsync(CardCreateDTO cardCreateDTO)
        {
            try
            {
                var cardToCreate = mapper.Map<Card>(cardCreateDTO);
                var accountExist = await context.BankAccounts.AnyAsync(x => x.Id == cardCreateDTO.BankAccountId);
                if(!accountExist)
                {
                    Log.Error($"Bank account with ID {cardCreateDTO.BankAccountId} not exists");
                    throw new BadHttpRequestException($"Bank account with ID {cardCreateDTO.BankAccountId} not exists");
                }

                var cardExist = await context.Cards.AnyAsync(x => x.CardNumber == cardCreateDTO.CardNumber);
                if(cardExist)
                {
                    Log.Error($"Card with Card Number {cardCreateDTO.CardNumber} already exists");
                    throw new BadHttpRequestException($"Card with Card Number {cardCreateDTO.CardNumber} already exists");
                }
                await context.Cards.AddAsync(cardToCreate);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while adding a card.");
                throw;
            }
        }
    }
}
