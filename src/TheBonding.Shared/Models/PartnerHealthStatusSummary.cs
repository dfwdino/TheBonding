namespace TheBonding.Shared.Models;

/// <summary>
/// A fully resolved health status entry — entity metadata combined with its decrypted record.
/// Used by IPartnerHealthStatusService return values and UI bindings.
/// </summary>
public class PartnerHealthStatusSummary
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    public DateTime TestedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public PartnerHealthStatusRecord Data { get; set; } = new();
}
