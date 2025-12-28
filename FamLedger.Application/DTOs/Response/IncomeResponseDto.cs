using FamLedger.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Application.DTOs.Response
{
    public class IncomeResponseDto
    {
        public int UserId { get; set; }
        public int FamilyId { get; set; }
        public string? TotalIncome { get; set; }
        public string? TotalRecurringIncome { get; set; }
        public int RecurringIncomeCount { get; set; }
        public string? PercentageDifference { get; set; }
        public List<IncomeItemDto> Incomes { get; set; }

    }
}
