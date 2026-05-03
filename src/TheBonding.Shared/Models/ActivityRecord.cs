namespace TheBonding.Shared.Models;

/// <summary>
/// Decrypted contents of Activity.DataBlob.
/// Serialized to/from JSON by the encryption service.
/// All list fields default to empty — never null — to simplify UI binding.
/// </summary>
public class ActivityRecord
{
    public string? Title { get; set; }

    /// <summary>ISO-8601 date string (yyyy-MM-dd). Also stored plaintext in entity for sorting.</summary>
    public string OccurredDate { get; set; } = string.Empty;

    /// <summary>LookupItem.Ids from the Activity Type category. Multiple allowed.</summary>
    public List<int> ActivityTypeIds { get; set; } = [];

    /// <summary>PartnerId — redundant with entity column, included here for export completeness.</summary>
    public int? PartnerId { get; set; }

    public int? DurationMinutes { get; set; }

    /// <summary>LookupItem.Id from the Location category. Null if not set.</summary>
    public int? LocationId { get; set; }

    /// <summary>LookupItem.Id from the Mood category. Null if not set.</summary>
    public int? MoodId { get; set; }

    /// <summary>LookupItem.Ids from the Contraception category. Multiple allowed.</summary>
    public List<int> ContraceptionIds { get; set; } = [];

    /// <summary>1–5 star rating. Null if not rated.</summary>
    public int? Rating { get; set; }

    public string? Notes { get; set; }
}
