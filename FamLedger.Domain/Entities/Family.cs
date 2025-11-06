using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Domain.Entities
{
    public class Family
    {
        public int FamilyId { get; set; }
        public string FamilyName {  get; set; } = string.Empty;
        public string FamilyCode {  get; set; } = string.Empty;
        public bool Status { get; set; } = true;
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; } 



        public ICollection<User> Users { get; set; }
        public ICollection<Income> Incomes { get; set; }
        public ICollection<Expense> Expenses { get; set; }
        public ICollection<Asset> Assets { get; set; }

    }
}
