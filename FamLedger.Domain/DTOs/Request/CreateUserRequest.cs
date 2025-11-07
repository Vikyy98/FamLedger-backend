namespace FamLedger.Domain.DTOs.Request
{
    public class CreateUserRequest
    {
        public string FullName {  get; set; } = string.Empty;
        public string FamilyName {  get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
