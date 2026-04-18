namespace FamLedger.Application.DTOs.Response
{
    public class IncomeResponseDto
    {
        public int FamilyId { get; set; }
        public string? TotalIncome { get; set; }
        public string? TotalRecurringIncome { get; set; }
        public int RecurringIncomeCount { get; set; }
        public string? PercentageDifference { get; set; }
        public List<IncomeItemDto> Incomes { get; set; } = new();
        public List<IncomeMonthlyTrendDto> MonthlyTrend { get; set; } = new();
    }

    public class IncomeMonthlyTrendDto
    {
        /// <summary>Short month label, e.g. "Jan", "Feb".</summary>
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Total { get; set; }
    }
}
