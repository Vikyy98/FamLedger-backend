using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamLedger.Domain.Entities
{
    public class Asset
    {
        public int Id { get; set; }

        public int FamilyId { get; set; }

        public string AssetName { get; set; } = string.Empty;

        public string AssetType { get; set; } = string.Empty;

        public int Value { get; set; }

        public int OwnerUserId { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public string? Remakrs { get; set; }

        public bool Status { get; set; } = true;

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public User User { get; set; }
        public Family Family { get; set; }
    }
}
