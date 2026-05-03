namespace TheBonding.Shared.Models;

/// <summary>
/// A fully resolved activity — entity metadata combined with its decrypted record.
/// Used by IActivityService return values and UI bindings.
/// </summary>
public class ActivitySummary
{
    public int Id { get; set; }
    public int? PartnerId { get; set; }
    public DateTime OccurredDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public ActivityRecord Data { get; set; } = new();
}
