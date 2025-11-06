using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Domain.Entities
{
    public class Loan
    {
        public int LoanId { get; set; }

        public int UserId {  get; set; }

        public int FamilyId {  get; set; }

        public string LoanType { get; set; } = string.Empty;

        public int Amount { get; set; }

        public string? LenderOrBorrowerName {  get; set; } = string.Empty;

        public string? LoanName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime DueDate {  get; set; }
         
        public decimal InterestRate { get; set;}

        public bool Status {  get; set; }

        public DateTime CreatedOn { get; set; } 

        public DateTime UpdatedOn { get; set; }

        public bool IsBorrowed { get; set; }


        public User User { get; set; }
        public Family Family { get; set; }
    }
}
