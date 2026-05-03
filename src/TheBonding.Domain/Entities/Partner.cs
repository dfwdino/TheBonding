namespace TheBonding.Domain.Entities;

/// <summary>
/// One row per partner profile.
/// DataBlob holds all personal partner details encrypted.
/// IsActive and LastUsedDate are plaintext for filtering and sorting
/// without requiring decryption.
/// </summary>
public class Partner
{
    public int Id { get; set; }

    /// <summary>Base64-encoded encrypted JSON blob — see PartnerRecord.</summary>
    public string DataBlob { get; set; } = string.Empty;

    /// <summary>
    /// True = active partner. False = inactive (soft-hidden).
    /// Stored plaintext so the partner list can filter without decryption.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Updated each time an activity is linked to this partner.</summary>
    public DateTime LastUsedDate { get; set; }

    public DateTime CreatedDate { get; set; }
}
