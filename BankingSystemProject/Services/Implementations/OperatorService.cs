using AutoMapper;
using BankingSystemProject.Core.DTOs;
using BankingSystemProject.Core.Enums;
using BankingSystemProject.Data;
using BankingSystemProject.Data.Models;
using BankingSystemProject.Data.Tables;
using BankingSystemProject.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

                var result = await userManager.CreateAsync(identityUser, credentials.Password);

                // ენამიდან გამომდინარე ინახავს სტრინგად როლის დასახელებას
                var roleName = Enum.GetName(typeof(Roles), credentials.Role);

                try
                {
                    if (result.Succeeded)
                    {
                        // ასინქრონულად ეძებს არსებობს თუ არა როლი ბაზაში
                        if (!await roleManager.RoleExistsAsync(roleName))
                        {
                            IdentityResult roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                            await userManager.AddToRoleAsync(identityUser, roleName);
                        }
                        else
                        {
                            await userManager.AddToRoleAsync(identityUser, roleName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "An error occurred while adding role to the user.");
                    throw new InvalidOperationException($"Role '{roleName}' does not exist.");
                }

                var roles = await roleManager.Roles.ToListAsync();
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while registering a user.");
                throw; // ხელახლა ვასროლინებ ექსეფშენს დეტალური ინფორმაციისთვის
            }
        }


        public async Task AddBankAccountAsync(BankAccountCreateDTO bankAccountCreateDTO)
        {
            try
            {
                var bankAccountToCreate = mapper.Map<BankAccount>(bankAccountCreateDTO);
                await context.BankAccounts.AddAsync(bankAccountToCreate);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while adding a bank account.");
                throw;
            }
        }

        public async Task AddCardAsync(CardCreateDTO cardCreateDTO)
        {
            try
            {
                var cardToCreate = mapper.Map<Card>(cardCreateDTO);
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
