namespace TheBonding.Shared.Models;

/// <summary>
/// Decrypted, fully resolved lookup item.
/// Combines LookupItem (DB entity) with its decrypted value and category name.
/// Used throughout the UI for dropdown lists and display.
/// </summary>
public class LookupItemRecord
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Decrypted display label.</summary>
    public string Value { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    /// <summary>True = shipped with the app. False = user added.</summary>
    public bool IsDefault { get; set; }
}
