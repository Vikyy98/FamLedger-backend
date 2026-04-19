using System.ComponentModel.DataAnnotations;

namespace FamLedger.Application.DTOs.Request
{
    public class UpdateMemberRoleRequest
    {
        /// <summary>Target role. Accepted values (case-insensitive): "Admin", "Member".</summary>
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
