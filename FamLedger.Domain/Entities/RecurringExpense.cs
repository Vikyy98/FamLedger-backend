using FamLedger.Domain.Enums;

namespace FamLedger.Domain.Entities
{
    public class RecurringExpense
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FamilyId { get; set; }
        public string? Description { get; set; }
        public ExpenseCategory Category { get; set; }
        public decimal Amount { get; set; }
        public bool Status { get; set; }
        public DateOnly StartDate { get; set; }
        public string? Frequency { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        public User? User { get; set; }
        public Family? Family { get; set; }
    }
}
