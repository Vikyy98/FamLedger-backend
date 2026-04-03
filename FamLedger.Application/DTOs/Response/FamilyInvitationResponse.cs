namespace FamLedger.Application.DTOs.Response;

public class FamilyInvitationResponse
{
    /// <summary>Plain text code — only returned once when generated.</summary>
    public string InvitationCode { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
}
