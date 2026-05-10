using FamLedger.Domain.Enums;

namespace FamLedger.Application.DTOs.Response
{
    public class DebtResponseDto
    {
        public int FamilyId { get; set; }
        public decimal TotalDebts { get; set; }
        public decimal TotalMonthlyEmi { get; set; }
        public string? TotalDebtsFormatted { get; set; }
        public string? TotalMonthlyEmiFormatted { get; set; }
        public int ActiveDebtCount { get; set; }
        public List<DebtItemDto> Debts { get; set; } = new();
        public List<DebtCategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
        public List<UpcomingEmiDto> UpcomingEmis { get; set; } = new();
    }

    public class DebtCategoryBreakdownDto
    {
        public DebtCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class UpcomingEmiDto
    {
        public int DebtId { get; set; }
        public string DebtName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateOnly DueDate { get; set; }
        public int DaysUntilDue { get; set; }
    }

    public class DebtCategoryOptionDto
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
