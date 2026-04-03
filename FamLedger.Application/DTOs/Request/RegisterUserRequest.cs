namespace FamLedger.Application.DTOs.Request;

/// <summary>
/// Registration is only allowed with a family: either create one or join with an invite code.
/// </summary>
public class RegisterUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>"createFamily" or "joinFamily"</summary>
    public string RegistrationMode { get; set; } = string.Empty;

    /// <summary>Required when RegistrationMode is createFamily.</summary>
    public string? FamilyName { get; set; }

    /// <summary>Required when RegistrationMode is joinFamily (plain code; hashed server-side).</summary>
    public string? InvitationCode { get; set; }
}
