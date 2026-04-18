using System.ComponentModel.DataAnnotations;
using FamLedger.Domain.Enums;

namespace FamLedger.Application.DTOs.Request
{
    /// <summary>
    /// Payload for creating / updating an expense.
    /// Note: UserId and FamilyId on this DTO are informational only; the
    /// service overwrites them with trusted JWT claim values on write.
    /// </summary>
    public class ExpenseRequestDto
    {
        public int UserId { get; set; }

        public int FamilyId { get; set; }

        [Required]
        [StringLength(200)]
        public string? Description { get; set; }

        [Required]
        public ExpenseCategory Category { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        public DateOnly? ExpenseDate { get; set; }
    }
}
