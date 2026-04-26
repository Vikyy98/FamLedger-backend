using System.ComponentModel;

namespace FamLedger.Domain.Enums
{
    public enum ExpenseType
    {
        [Description("Recurring expense")]
        Recurring = 1,
        [Description("One time expense")]
        OneTime = 2
    }
}
