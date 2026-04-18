using FamLedger.Domain.Enums;

namespace FamLedger.Application.DTOs.Response
{
    public class ExpenseItemDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FamilyId { get; set; }
        public string? Description { get; set; }
        public ExpenseCategory Category { get; set; }
        public decimal Amount { get; set; }
        public DateOnly? ExpenseDate { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
