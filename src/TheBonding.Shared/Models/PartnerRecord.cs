namespace TheBonding.Shared.Models;

/// <summary>
/// Decrypted contents of Partner.DataBlob.
/// Serialized to/from JSON by the encryption service.
/// </summary>
public class PartnerRecord
{
    /// <summary>Required — a partner must have a name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>LookupItem.Id from the Gender Identity category. Null if not set.</summary>
    public int? GenderIdentityId { get; set; }

    /// <summary>LookupItem.Id from the Sexual Orientation category. Null if not set.</summary>
    public int? SexualOrientationId { get; set; }

    /// <summary>LookupItem.Id from the Relationship Type category. Null if not set.</summary>
    public int? RelationshipTypeId { get; set; }

    /// <summary>ISO-8601 date string (yyyy-MM-dd). Date the user met this partner.</summary>
    public string? MetDate { get; set; }

    /// <summary>Free-text field for role/dynamic preferences.</summary>
    public string? RolePreferences { get; set; }

    public string? Notes { get; set; }
}
