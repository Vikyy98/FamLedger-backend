using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Domain.Enums
{
    public enum IncomeSource
    {
        [Description("Company Salary")]
        Salary = 1,
        [Description("Freelance Client")]
        Freelance = 2,
        [Description("Rental Property")]
        Rental = 3,
        [Description("Mutual Fund / Stock Returns")]
        Stocks = 4,
        [Description("Bank Interest / Fixed Deposit")]
        Interests = 5
    }
}
