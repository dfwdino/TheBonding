namespace TheBonding.Shared.Models;

/// <summary>
/// A fully resolved partner — entity metadata combined with its decrypted record.
/// Used by IPartnerService return values and UI bindings.
/// </summary>
public class PartnerSummary
{
    public int Id { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastUsedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public PartnerRecord Data { get; set; } = new();
}
