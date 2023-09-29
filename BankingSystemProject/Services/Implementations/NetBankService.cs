using AutoMapper;
using BankingSystem.Core.DTOs;
using BankingSystem.Services.Interfaces;
using BankingSystemProject.Core.DTOs;
using BankingSystemProject.Data;
using BankingSystemProject.Data.Tables;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BankingSystemProject.Services.Implementations
{
    public class NetBankService : INetBankService
    {
        private readonly BankingDbContext context;
        private readonly IMapper mapper;

        public NetBankService(BankingDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        public async Task<List<BankAccountResponseDTO>> GetBankAccountsForUserAsync(string userId)
        {
            var bankAccounts = await context.BankAccounts
                .Where(account => account.UserId == userId)
                .ToListAsync();

            var bankAccountResponseDTOs = mapper.Map<List<BankAccountResponseDTO>>(bankAccounts);

            return bankAccountResponseDTOs;
        }

        public async Task<List<CardResponseDTO>> GetCardsForUserAsync(string userId)
        {
            var cards = await context.Cards
                .Join(
                    context.BankAccounts,
                    card => card.BankAccountId,
                    bankAccount => bankAccount.Id,
                    (card, bankAccount) => new { Card = card, BankAccount = bankAccount })
                .Where(joinResult => joinResult.BankAccount.UserId == userId)
                .Select(joinResult => joinResult.Card)
                .ToListAsync();

            var cardResponseDTOs = mapper.Map<List<CardResponseDTO>>(cards);

            return cardResponseDTOs;
        }


        public async Task CreateTransactionAsync(TransactionCreateDTO transactionCreateDTO)
        {
            try
            {
                var senderAccount = await context.BankAccounts
                    .Include(ba => ba.User)
                    .Where(x => x.IBAN == transactionCreateDTO.SenderBankAccountIBAN)
                    .FirstOrDefaultAsync();

                if(transactionCreateDTO.UserId != senderAccount.UserId)
                {
                    Log.Error("The sender account must belong to the sender user");
                    throw new BadHttpRequestException("The sender account must belong to the sender user");
                }

                var receiverAccount = await context.BankAccounts
                    .Where(x => x.IBAN == transactionCreateDTO.ReceiverBankAccountIBAN)
                    .FirstOrDefaultAsync();

                if (senderAccount == null)
                {
                    Log.Error("Sender account not found.");
                    throw new BadHttpRequestException("Sender account not found.");
                }

                if (receiverAccount == null)
                {
                    Log.Error("Receiver account not found.");
                    throw new BadHttpRequestException("Receiver account not found.");
                }

                // ინახავს გამგზავნი და მიმღები ანგარიშების ვალუტებს
                var senderCurrency = senderAccount.CurrencyCode;
                var receiverCurrency = receiverAccount.CurrencyCode;

                decimal amountToTransfer = transactionCreateDTO.Amount;
                decimal commissionRate = 0.01m; // 1%
                decimal commissionAmount = 0;
                decimal finalCommissionAmount = 0;

                if (senderAccount.UserId == receiverAccount.UserId)
                {
                    // ამოწმებს ტრანზაქციის ვალუტა და გამგზავნის ანგარიშის ვალუტა იდენტურია თუ არა
                    if (senderCurrency != transactionCreateDTO.CurrencyCode)
                    {
                        // აბრუნებს შესაბამის გაცვლით კურსს
                        var exchangeRate = await context.ExchangeRates
                            .Where(rate => rate.FromCurrencyCode == transactionCreateDTO.CurrencyCode && rate.ToCurrencyCode == senderCurrency)
                            .FirstOrDefaultAsync() ?? throw new BadHttpRequestException("Exchange rate not found.");

                        decimal rate = exchangeRate.Rate;
                        decimal convertedAmount = amountToTransfer * rate;

                        if (senderAccount.Amount < convertedAmount)
                        {
                            Log.Error("Insufficient balance");
                            throw new BadHttpRequestException("Insufficient balance.");
                        }

                        senderAccount.Amount -= convertedAmount;

                        // ამოწმებს მიმღების ანგარიშის ვალუტა განსხვავდება თუ არა ტრანზქციის ვალუტისგან
                        if (receiverCurrency != transactionCreateDTO.CurrencyCode)
                        {
                            // აბრუნებს შესაბამის გაცვლით კურსს
                            var receiverExchangeRate = await context.ExchangeRates
                                .Where(rate => rate.FromCurrencyCode == transactionCreateDTO.CurrencyCode && rate.ToCurrencyCode == receiverCurrency)
                                .FirstOrDefaultAsync() ?? throw new BadHttpRequestException("Exchange rate not found.");

                            decimal receiverRate = receiverExchangeRate.Rate;
                            decimal finalConvertedAmount = amountToTransfer * receiverRate;
                            receiverAccount.Amount += finalConvertedAmount;
                        }
                        else
                        {
                            receiverAccount.Amount += amountToTransfer;
                        }
                    }
                    else if (receiverCurrency != transactionCreateDTO.CurrencyCode)
                    {
                        // გამგზავნის ანგარიშის ვალუტა ტრანზქციის ვალუტის იდენტურია,
                        // თუმცა მიმღების ანგარიში განსხვავდება.

                        var receiverExchangeRate = await context.ExchangeRates
                            .Where(rate => rate.FromCurrencyCode == transactionCreateDTO.CurrencyCode && rate.ToCurrencyCode == receiverCurrency)
                            .FirstOrDefaultAsync() ?? throw new BadHttpRequestException("Exchange rate not found.");

                        decimal receiverRate = receiverExchangeRate.Rate;
                        decimal convertedAmount = amountToTransfer * receiverRate;
                        if (senderAccount.Amount < convertedAmount)
                        {
                            Log.Error("Insufficient balance");
                            throw new BadHttpRequestException("Insufficient balance.");
                        }
                        senderAccount.Amount -= amountToTransfer;
                        receiverAccount.Amount += convertedAmount;
                    }
                    else
                    {
                        // მიმღებისა და გამგზავნი მომხმარებლის ვალუტები იდენტურია და შეესაბამება ტრანზაქციის ვალუტას.
                        if (senderAccount.Amount < amountToTransfer)
                        {
                            Log.Error("Insufficient balance");
                            throw new BadHttpRequestException("Insufficient balance.");
                        }
                        senderAccount.Amount -= amountToTransfer;
                        receiverAccount.Amount += amountToTransfer;
                    }
                }
                else
                {
                    // თანხის გადარიცხვა სხვის ანგარიშზე
                    // საჭიროა საკომისიოს გათვალისწინება
                    commissionAmount = amountToTransfer * commissionRate;
                    finalCommissionAmount = commissionAmount + 0.5m;

                    if (senderCurrency != transactionCreateDTO.CurrencyCode)
                    {
                        var exchangeRate = await context.ExchangeRates
                            .Where(rate => rate.FromCurrencyCode == transactionCreateDTO.CurrencyCode && rate.ToCurrencyCode == senderCurrency)
                            .FirstOrDefaultAsync() ?? throw new BadHttpRequestException("Exchange rate not found.");

                        decimal rate = exchangeRate.Rate;
                        decimal convertedAmount = amountToTransfer * rate;

                        if (senderAccount.Amount < convertedAmount)
                        {
                            Log.Error("Insufficient balance");
                            throw new BadHttpRequestException("Insufficient balance.");
                        }

                        commissionAmount *= exchangeRate.Rate;

                        senderAccount.Amount -= (convertedAmount + finalCommissionAmount);

                        if (receiverCurrency != transactionCreateDTO.CurrencyCode)
                        {
                            var receiverExchangeRate = await context.ExchangeRates
                                .Where(rate => rate.FromCurrencyCode == transactionCreateDTO.CurrencyCode && rate.ToCurrencyCode == receiverCurrency)
                                .FirstOrDefaultAsync() ?? throw new BadHttpRequestException("Exchange rate not found.");

                            decimal receiverRate = receiverExchangeRate.Rate;
                            decimal finalConvertedAmount = amountToTransfer * receiverRate;

                            receiverAccount.Amount += finalConvertedAmount;
                        }
                        else
                        {
                            receiverAccount.Amount += amountToTransfer;
                        }
                    }
                    else if (receiverCurrency != transactionCreateDTO.CurrencyCode)
                    {
                        var receiverExchangeRate = await context.ExchangeRates
                            .Where(rate => rate.FromCurrencyCode == transactionCreateDTO.CurrencyCode && rate.ToCurrencyCode == receiverCurrency)
                            .FirstOrDefaultAsync() ?? throw new Exception("Exchange rate not found.");

                        decimal receiverRate = receiverExchangeRate.Rate;
                        decimal convertedAmount = amountToTransfer * receiverRate;

                        if (senderAccount.Amount < convertedAmount)
                        {
                            Log.Error("Insufficient balance");
                            throw new BadHttpRequestException("Insufficient balance.");
                        }

                        senderAccount.Amount -= (amountToTransfer + finalCommissionAmount);
                        receiverAccount.Amount += convertedAmount;
                    }
                    else
                    {
                        if (senderAccount.Amount < amountToTransfer)
                        {
                            Log.Error("Insufficient balance");
                            throw new BadHttpRequestException("Insufficient balance.");
                        }
                        senderAccount.Amount -= (amountToTransfer + finalCommissionAmount);
                        receiverAccount.Amount += amountToTransfer;
                    }
                }

                // ტრანზაქციის შექმნა, რადგან შედარებით რთული ობიექტია, არ ვიყენებ AutoMapper-ს
                var transaction = new Transaction
                {
                    Amount = amountToTransfer,
                    CommisionAmount = finalCommissionAmount,
                    CurrencyCode = transactionCreateDTO.CurrencyCode,
                    SenderBankAccountIBAN = senderAccount.IBAN,
                    ReceiverBankAccountIBAN = receiverAccount.IBAN,
                    SenderUserId = senderAccount.UserId
                };

                await context.Transactions.AddAsync(transaction);
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex.Message, "An error occurred while saving the entity changes.");
                throw new DbUpdateException("An error occurred while saving the entity changes.", ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, "An error occurred in the NetBankService.");
                throw new Exception (ex.Message);
            }
        }
    }
}