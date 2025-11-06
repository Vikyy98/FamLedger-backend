using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Domain.Entities
{
    public class Income
    {
        public int IncomeId {get; set; }

        public int UserId { get; set; }

        public int FamilyId { get; set; }

        public string IncomeType {get; set; } = string.Empty;
        
        public int Amount {get; set; }

        public bool Status { get; set; }

        public DateTime CreatedOn { get; set; } 

        public DateTime UpdatedOn { get; set; } 


        public User User { get; set; }
        public Family Family { get; set; }


    }
}
