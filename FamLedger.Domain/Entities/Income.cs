using FamLedger.Domain.Enums;

namespace FamLedger.Domain.Entities
{
    public class Income
    {
        public int IncomeId { get; set; }
        public int UserId { get; set; }
        public int FamilyId { get; set; }
        public string? Source { get; set; }
        public IncomeCategory Category { get; set; }
        public IncomeType Type { get; set; }
        public decimal Amount { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public User User { get; set; }
        public Family Family { get; set; }
    }
}
