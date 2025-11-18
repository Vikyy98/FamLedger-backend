using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Domain.Enums
{
    public enum IncomeCategory
    {
        [Description("Job")]
        Job = 1,
        [Description("Side Business / Freelancing")]
        Business = 2,
        [Description("Rental Income")]
        Rental = 3,
        [Description("Investment Income (Dividends / Interest / Trading Gains)")]
        Stocks = 4,
        [Description("Gifts / Support / Others")]
        Others = 5
    }
}
