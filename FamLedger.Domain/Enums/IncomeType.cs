using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Domain.Enums
{
    public enum IncomeType
    {
        [Description("Recurring income")]
        Recurring = 1,
        [Description("One time income")]
        OneTime = 2
    }
}
