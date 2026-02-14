using FamLedger.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Application.DTOs.Response
{
    public class IncomeItemDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FamilyId { get; set; }
        public string? Source { get; set; }
        public IncomeType Type { get; set; }
        public string Frequency { get; set; } = "ONETIME";
        public DateOnly? DateReceived { get; set; }
        public decimal Amount { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
