using System.ComponentModel;

namespace FamLedger.Domain.Enums
{
    public enum ExpenseCategory
    {
        [Description("Food")]
        Food = 1,

        [Description("Housing")]
        Housing = 2,

        [Description("Transport")]
        Transport = 3,

        [Description("Utilities")]
        Utilities = 4,

        [Description("Entertainment")]
        Entertainment = 5,

        [Description("Medical")]
        Medical = 6,

        [Description("Education")]
        Education = 7,

        [Description("Other")]
        Other = 99,
    }
}
