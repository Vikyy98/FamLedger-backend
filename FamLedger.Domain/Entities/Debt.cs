using FamLedger.Domain.Enums;

namespace FamLedger.Domain.Entities
{
    public class Debt
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int FamilyId { get; set; }

        public string DebtName { get; set; } = string.Empty;

        public DebtCategory Category { get; set; }

        public string? LenderName { get; set; }

        public decimal PrincipalAmount { get; set; }

        public decimal RemainingAmount { get; set; }

        public decimal InterestRate { get; set; }

        public decimal MonthlyEmi { get; set; }

        // Day-of-month the EMI is due (1..28). Clamped to 28 to dodge Feb edge cases.
        public int EmiDayOfMonth { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public DebtStatus Status { get; set; } = DebtStatus.Active;

        public string? Notes { get; set; }

        // Back-reference to the RecurringExpense row that mirrors the EMI for this debt.
        // Null when the user opted to track the debt without auto-creating an EMI expense.
        public int? LinkedRecurringExpenseId { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public User? User { get; set; }
        public Family? Family { get; set; }
        public RecurringExpense? LinkedRecurringExpense { get; set; }
    }
}
