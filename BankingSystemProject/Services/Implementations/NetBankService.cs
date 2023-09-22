using BankingSystem.Core.DTOs;
using BankingSystem.Services.Interfaces;
using BankingSystemProject.Data;
using BankingSystemProject.Data.Tables;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BankingSystemProject.Services.Implementations
{
    public class NetBankService : INetBankService
    {
        private readonly BankingDbContext context;

        public NetBankService(BankingDbContext context)
        {
            this.context = context;
        }
        public async Task<List<BankAccount>> GetBankAccountsForUserAsync(string userId)
        {   
            return await context.BankAccounts.Where(account => account.UserId == userId).ToListAsync();
        }

        public async Task<List<Card>> GetCardsForUserAsync(string userId)
        {
            return await context.Cards
                .Join(
                    context.BankAccounts,
                    card => card.BankAccountId,
                    bankAccount => bankAccount.Id,
                    (card, bankAccount) => new { Card = card, BankAccount = bankAccount })
                .Where(joinResult => joinResult.BankAccount.UserId == userId)
                .Select(joinResult => joinResult.Card)
                .ToListAsync();
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
                    throw new BadHttpRequestException("The sender account must belong to the sender user");
                }

                var receiverAccount = await context.BankAccounts
                    .Where(x => x.IBAN == transactionCreateDTO.ReceiverBankAccountIBAN)
                    .FirstOrDefaultAsync();

                if (senderAccount == null || receiverAccount == null)
                {
                    throw new Exception("Sender or receiver account not found.");
                }

                // განსაზღვრავს გამგზავნი და მიმღები ანგარიშების ვალუტებს
                var senderCurrency = senderAccount.CurrencyCode;
                var receiverCurrency = receiverAccount.CurrencyCode;

                decimal amountToTransfer = transactionCreateDTO.Amount;
                decimal commissionRate = 0.01m; // 1%
                decimal commissionAmount = 0;
                decimal finalCommissionAmount = 0;

                if (senderAccount.UserId == receiverAccount.UserId)
                {
                    // Check if sender has sufficient balance
                    if (senderAccount.Amount < amountToTransfer)
                    {
                        throw new Exception("Insufficient balance.");
                    }

                    // Check if sender's currency is different from the transaction currency
                    if (senderCurrency != transactionCreateDTO.CurrencyCode)
                    {
                        // Find the exchange rate from sender's currency to the transaction currency
                        var exchangeRate = await context.ExchangeRates
                            .Where(rate => rate.FromCurrencyCode == transactionCreateDTO.CurrencyCode && rate.ToCurrencyCode == senderCurrency)
                            .FirstOrDefaultAsync() ?? throw new Exception("Exchange rate not found.");

                        decimal rate = exchangeRate.Rate;
                        decimal convertedAmount = amountToTransfer * rate;

                        // Deduct the converted amount from the sender's account
                        senderAccount.Amount -= convertedAmount;

                        // Check if receiver's currency is also different from the transaction currency
                        if (receiverCurrency != transactionCreateDTO.CurrencyCode)
                        {
                            // Find the exchange rate from the transaction currency to receiver's currency
                            var receiverExchangeRate = await context.ExchangeRates
                                .Where(rate => rate.FromCurrencyCode == receiverCurrency && rate.ToCurrencyCode == transactionCreateDTO.CurrencyCode)
                                .FirstOrDefaultAsync() ?? throw new Exception("Exchange rate not found.");

                            decimal receiverRate = receiverExchangeRate.Rate;
                            decimal finalConvertedAmount = convertedAmount * receiverRate;

                            // Add the final converted amount to the receiver's account
                            receiverAccount.Amount += finalConvertedAmount;
                        }
                        else
                        {
                            // Add the converted amount (in the transaction currency) to the receiver's account
                            receiverAccount.Amount += amountToTransfer;
                        }
                    }
                    else if (receiverCurrency != transactionCreateDTO.CurrencyCode)
                    {
                        // Sender's currency is the same as the transaction currency,
                        // but receiver's currency is different. Convert and add to receiver's account.

                        // Find the exchange rate from transaction currency to receiver's currency
                        var receiverExchangeRate = await context.ExchangeRates
                            .Where(rate => rate.FromCurrencyCode == transactionCreateDTO.CurrencyCode && rate.ToCurrencyCode == receiverCurrency)
                            .FirstOrDefaultAsync() ?? throw new Exception("Exchange rate not found.");

                        decimal receiverRate = receiverExchangeRate.Rate;
                        decimal convertedAmount = amountToTransfer * receiverRate;

                        // Add the converted amount to the receiver's account
                        senderAccount.Amount -= amountToTransfer;
                        receiverAccount.Amount += convertedAmount;
                    }
                    else
                    {
                        // Both sender and receiver have the same currency, no need for conversion
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

                    if (senderAccount.Amount < amountToTransfer + commissionAmount)
                    {
                        throw new Exception("Insufficient balance.");
                    }

                    // Check if sender's currency is different from the transaction currency
                    if (senderCurrency != transactionCreateDTO.CurrencyCode)
                    {
                        // Find the exchange rate from sender's currency to the transaction currency
                        var exchangeRate = await context.ExchangeRates
                            .Where(rate => rate.FromCurrencyCode == transactionCreateDTO.CurrencyCode && rate.ToCurrencyCode == senderCurrency)
                            .FirstOrDefaultAsync() ?? throw new Exception("Exchange rate not found.");

                        decimal rate = exchangeRate.Rate;
                        decimal convertedAmount = amountToTransfer * rate;
                        commissionAmount *= exchangeRate.Rate;

                        // Deduct the converted amount from the sender's account
                        senderAccount.Amount -= (convertedAmount + finalCommissionAmount);

                        // Check if receiver's currency is also different from the transaction currency
                        if (receiverCurrency != transactionCreateDTO.CurrencyCode)
                        {
                            // Find the exchange rate from the transaction currency to receiver's currency
                            var receiverExchangeRate = await context.ExchangeRates
                                .Where(rate => rate.FromCurrencyCode == receiverCurrency && rate.ToCurrencyCode == transactionCreateDTO.CurrencyCode)
                                .FirstOrDefaultAsync() ?? throw new Exception("Exchange rate not found.");

                            decimal receiverRate = receiverExchangeRate.Rate;
                            decimal finalConvertedAmount = convertedAmount * receiverRate;

                            // Add the final converted amount to the receiver's account
                            receiverAccount.Amount += finalConvertedAmount;
                        }
                        else
                        {
                            // Add the converted amount (in the transaction currency) to the receiver's account
                            receiverAccount.Amount += amountToTransfer;
                        }
                    }
                    else if (receiverCurrency != transactionCreateDTO.CurrencyCode)
                    {
                        // Sender's currency is the same as the transaction currency,
                        // but receiver's currency is different. Convert and add to receiver's account.

                        // Find the exchange rate from transaction currency to receiver's currency
                        var receiverExchangeRate = await context.ExchangeRates
                            .Where(rate => rate.FromCurrencyCode == transactionCreateDTO.CurrencyCode && rate.ToCurrencyCode == receiverCurrency)
                            .FirstOrDefaultAsync() ?? throw new Exception("Exchange rate not found.");

                        decimal receiverRate = receiverExchangeRate.Rate;
                        decimal convertedAmount = amountToTransfer * receiverRate;

                        // Add the converted amount to the receiver's account
                        senderAccount.Amount -= (amountToTransfer + finalCommissionAmount);
                        receiverAccount.Amount += convertedAmount;
                    }
                    else
                    {
                        // Both sender and receiver have the same currency, no need for conversion
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
                Log.Error(ex, "An error occurred while saving the entity changes.");
                throw new Exception("An error occurred while saving the entity changes.", ex);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred in the NetBankService.");
                throw;
            }
        }
    }
}