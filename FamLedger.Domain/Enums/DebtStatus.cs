using System.ComponentModel;

namespace FamLedger.Domain.Enums
{
    public enum DebtStatus
    {
        [Description("Active")]
        Active = 1,

        [Description("Paid off")]
        PaidOff = 2,

        [Description("Archived")]
        Archived = 3,
    }
}
