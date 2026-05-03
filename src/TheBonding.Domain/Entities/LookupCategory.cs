namespace TheBonding.Domain.Entities;

/// <summary>
/// Groups of lookup values (e.g., "Activity Type", "Mood", "Gender Identity").
/// Names are plaintext — category labels are structural, not sensitive.
/// Seeded by DatabaseInitializer at first launch.
///
/// IsSystem = true means the user cannot delete the category itself,
/// but can still add, edit, and delete individual items within it.
/// </summary>
public class LookupCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>True = category cannot be deleted by the user.</summary>
    public bool IsSystem { get; set; }
}
