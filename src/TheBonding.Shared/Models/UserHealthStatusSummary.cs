namespace TheBonding.Shared.Models;

/// <summary>
/// Fully resolved user health record — entity metadata + decrypted record.
/// </summary>
public class UserHealthStatusSummary
{
    public int        Id          { get; set; }
    public DateTime   TestedDate  { get; set; }
    public DateTime   CreatedDate { get; set; }
    public PartnerHealthStatusRecord Data { get; set; } = new();
}
