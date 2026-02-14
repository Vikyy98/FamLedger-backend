using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamLedger.Domain.Enums;

namespace FamLedger.Application.DTOs.Request
{
    public class IncomeRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int FamilyId { get; set; }

        [Required]
        [StringLength(200)]
        public string? Source { get; set; }

        public IncomeType Type { get; set; }

        public string Frequency { get; set; } = "ONETIME";

        [Required]
        [Range(0.00, double.MaxValue)]
        public decimal Amount { get; set; }

        public DateOnly? DateReceived { get; set; }
    }
}
