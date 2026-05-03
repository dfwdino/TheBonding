namespace TheBonding.Domain.Entities;

/// <summary>
/// One row per health/STI test entry per partner.
/// Full history is preserved — inserts and hard deletes only, no updates.
/// TestedDate is plaintext for chronological sorting without decryption.
/// </summary>
public class PartnerHealthStatus
{
    public int Id { get; set; }

    public int PartnerId { get; set; }

    /// <summary>Base64-encoded encrypted JSON blob — see PartnerHealthStatusRecord.</summary>
    public string DataBlob { get; set; } = string.Empty;

    /// <summary>Stored plaintext for sorting. Also present inside DataBlob.</summary>
    public DateTime TestedDate { get; set; }

    public DateTime CreatedDate { get; set; }
}
