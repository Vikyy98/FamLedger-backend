using FamLedger.Domain.Enums;

namespace FamLedger.Application.DTOs.Response
{
    public class DebtItemDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FamilyId { get; set; }
        public string DebtName { get; set; } = string.Empty;
        public DebtCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? LenderName { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal InterestRate { get; set; }
        public decimal MonthlyEmi { get; set; }
        public int EmiDayOfMonth { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateOnly? NextEmiDate { get; set; }
        public DebtStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsEmiTrackedAsExpense { get; set; }
        public decimal ProgressPercent { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
