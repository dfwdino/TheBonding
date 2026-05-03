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

    public int? DurationMinutes { get; set; }

    /// <summary>LookupItem.Id from the Time of Day category.</summary>
    public int? TimeOfDayId { get; set; }

    /// <summary>True when this was a solo session — no partner involved.</summary>
    public bool IsSolo { get; set; }

    /// <summary>Partner IDs involved in this event. Empty when IsSolo = true.</summary>
    public List<int> PartnerIds { get; set; } = [];

    /// <summary>Kept for backward compatibility with records saved before multi-partner support.</summary>
    public int? PartnerId { get; set; }

    /// <summary>LookupItem.Ids from the Activity Type category.</summary>
    public List<int> ActivityTypeIds { get; set; } = [];

    /// <summary>LookupItem.Ids from the Position category.</summary>
    public List<int> PositionIds { get; set; } = [];

    /// <summary>LookupItem.Ids from the Role category.</summary>
    public List<int> RoleIds { get; set; } = [];

    /// <summary>LookupItem.Ids from the Contraception category.</summary>
    public List<int> ContraceptionIds { get; set; } = [];

    /// <summary>LookupItem.Ids from the Climax category.</summary>
    public List<int> ClimaxIds { get; set; } = [];

    /// <summary>LookupItem.Id from the Location category.</summary>
    public int? LocationId { get; set; }

    /// <summary>1–5 star rating. Null if not rated.</summary>
    public int? Rating { get; set; }

    public string? Notes { get; set; }
}
