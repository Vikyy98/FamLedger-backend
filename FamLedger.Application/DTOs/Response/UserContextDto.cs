namespace FamLedger.Application.DTOs.Response
{
    public class UserContextDto
    {
        public int? UserId { get; set; }
        public int? FamilyId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; } = false;
    }
}