﻿using BankingSystemProject.Core.Enums;

namespace BankingSystemProject.Core.DTOs
{
    public class WithdrawMoneyControllerDTO
    {
        public decimal Amount { get; set; }
        public Currencies CurrencyCode { get; set; }
    }
}
