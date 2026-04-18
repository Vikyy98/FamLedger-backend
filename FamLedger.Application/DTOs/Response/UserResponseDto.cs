namespace FamLedger.Application.DTOs.Response
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public int? FamilyId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool Status { get; set; } = true;
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
