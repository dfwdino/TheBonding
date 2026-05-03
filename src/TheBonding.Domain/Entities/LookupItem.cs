namespace TheBonding.Domain.Entities;

/// <summary>
/// An individual lookup value within a category.
/// EncryptedValue is the user-visible label stored as AES-256 encrypted Base64.
/// Values are encrypted because user-created labels may reveal personal preferences.
///
/// Seeding:
///   Default items are seeded programmatically at first-launch setup (after Key2
///   is available for encryption). They are NOT inserted by DatabaseInitializer.
///
/// IsDefault = true means the item was seeded with the app.
/// The user can edit or delete any item, including defaults.
/// </summary>
public class LookupItem
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    /// <summary>Base64-encoded encrypted label string.</summary>
    public string EncryptedValue { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    /// <summary>True = shipped with the app. False = added by the user.</summary>
    public bool IsDefault { get; set; } = true;
}
