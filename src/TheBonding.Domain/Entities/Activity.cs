namespace TheBonding.Domain.Entities;

/// <summary>
/// One row per logged activity event.
/// DataBlob holds all activity details encrypted.
/// OccurredDate and PartnerId are plaintext for sorting and filtering
/// without requiring decryption.
/// </summary>
public class Activity
{
    public int Id { get; set; }

    /// <summary>
    /// Nullable — an activity may be logged without a linked partner.
    /// Stored plaintext so the activity list can filter by partner without decryption.
    /// </summary>
    public int? PartnerId { get; set; }

    /// <summary>Base64-encoded encrypted JSON blob — see ActivityRecord.</summary>
    public string DataBlob { get; set; } = string.Empty;

    /// <summary>Stored plaintext for chronological sorting. Also present inside DataBlob.</summary>
    public DateTime OccurredDate { get; set; }

    public DateTime CreatedDate { get; set; }
}
