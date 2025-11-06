using System.ComponentModel.DataAnnotations;

namespace FamLedger.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public int FamilyId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool Status { get; set; } = true;
        public DateTime CreatedOn { get; set; } 
        public DateTime UpdatedOn { get; set; }

        public Family Family { get; set; }
        public ICollection<Income> Incomes { get; set; }
        public ICollection<Expense> Expenses { get; set; }
        public ICollection<Asset> Assets { get; set; }

    }
}
