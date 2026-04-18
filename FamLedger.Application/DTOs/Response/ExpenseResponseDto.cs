using FamLedger.Domain.Enums;

namespace FamLedger.Application.DTOs.Response
{
    public class ExpenseResponseDto
    {
        public int FamilyId { get; set; }
        public string? TotalExpense { get; set; }
        public string? PercentageDifference { get; set; }
        public List<ExpenseItemDto> Expenses { get; set; } = new();
        public List<CategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
        public List<ExpenseMonthlyTrendDto> MonthlyTrend { get; set; } = new();
    }

    public class CategoryBreakdownDto
    {
        public ExpenseCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ExpenseMonthlyTrendDto
    {
        /// <summary>Short month label, e.g. "Jan", "Feb".</summary>
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Total { get; set; }
    }
}
