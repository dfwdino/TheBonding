namespace TheBonding.Shared.Models;

/// <summary>
/// Decrypted contents of PartnerHealthStatus.DataBlob.
/// Serialized to/from JSON by the encryption service.
/// One record per test/status visit. History is never overwritten.
/// </summary>
public class PartnerHealthStatusRecord
{
    /// <summary>LookupItem.Id from the Health Test Type category. Null if not set.</summary>
    public int? HealthTestTypeId { get; set; }

    /// <summary>
    /// Test result — free text so the user can record whatever the test showed.
    /// Examples: "Negative", "Positive", "Inconclusive", "Not tested yet"
    /// </summary>
    public string? Result { get; set; }

    /// <summary>ISO-8601 date string (yyyy-MM-dd). Also stored plaintext in the entity for sorting.</summary>
    public string TestedDate { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
