using System.ComponentModel.DataAnnotations;
using FamLedger.Domain.Enums;

namespace FamLedger.Application.DTOs.Request
{
    /// <summary>
    /// Payload for creating / updating a debt.
    /// UserId/FamilyId on this DTO are informational only; the service overrides them
    /// with trusted JWT claim values on write.
    /// </summary>
    public class DebtRequestDto
    {
        public int UserId { get; set; }

        public int FamilyId { get; set; }

        [Required]
        [StringLength(120)]
        public string DebtName { get; set; } = string.Empty;

        [Required]
        public DebtCategory Category { get; set; }

        [StringLength(200)]
        public string? LenderName { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Principal must be greater than zero.")]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Range(0.0, double.MaxValue, ErrorMessage = "Remaining amount cannot be negative.")]
        public decimal RemainingAmount { get; set; }

        [Required]
        [Range(0.0, 100.0, ErrorMessage = "Interest rate must be between 0 and 100.")]
        public decimal InterestRate { get; set; }

        [Required]
        [Range(0.0, double.MaxValue, ErrorMessage = "Monthly EMI cannot be negative.")]
        public decimal MonthlyEmi { get; set; }

        [Required]
        [Range(1, 28, ErrorMessage = "EMI day-of-month must be between 1 and 28.")]
        public int EmiDayOfMonth { get; set; }

        [Required]
        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        // When true, the API will auto-create / maintain a linked RecurringExpense
        // so each month's EMI shows up in the family expenses ledger.
        public bool TrackEmiAsExpense { get; set; } = true;
    }
}
