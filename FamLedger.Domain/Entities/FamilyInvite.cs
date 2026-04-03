namespace FamLedger.Domain.Entities;

/// <summary>
/// Single-use invite for joining a family. Plain code is shown once; only SHA-256 hash is stored.
/// At most one active row per family is enforced by deleting previous rows when a new code is issued.
/// </summary>
public class FamilyInvite
{
    public int Id { get; set; }
    public int FamilyId { get; set; }
    /// <summary>SHA-256 (hex) of the uppercase normalized plain code.</summary>
    public string CodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public int CreatedByUserId { get; set; }

    public Family? Family { get; set; }
}
