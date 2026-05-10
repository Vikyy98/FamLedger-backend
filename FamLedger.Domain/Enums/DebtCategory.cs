using System.ComponentModel;

namespace FamLedger.Domain.Enums
{
    public enum DebtCategory
    {
        [Description("Mortgage")]
        Mortgage = 1,

        [Description("Auto Loan")]
        AutoLoan = 2,

        [Description("Credit Card")]
        CreditCard = 3,

        [Description("Education")]
        Education = 4,

        [Description("Personal")]
        Personal = 5,

        [Description("Other")]
        Other = 99,
    }
}
