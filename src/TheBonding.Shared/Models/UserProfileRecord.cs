namespace TheBonding.Shared.Models;

/// <summary>
/// Decrypted contents of UserProfile.DataBlob.
/// Serialized to/from JSON by the encryption service.
/// All fields are optional — the user fills in only what they want.
/// </summary>
public class UserProfileRecord
{
    public string? DisplayName { get; set; }

    /// <summary>ISO-8601 date string (yyyy-MM-dd). Null if not provided.</summary>
    public string? DateOfBirth { get; set; }

    /// <summary>LookupItem.Id from the Gender Identity category. Null if not set.</summary>
    public int? GenderIdentityId { get; set; }

    /// <summary>LookupItem.Id from the Sexual Orientation category. Null if not set.</summary>
    public int? SexualOrientationId { get; set; }

    public string? Notes { get; set; }
}
