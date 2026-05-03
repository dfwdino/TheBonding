namespace TheBonding.Domain.Entities;

/// <summary>
/// One row per STI/STD test entry for the app owner themselves.
/// Mirrors PartnerHealthStatus but without a foreign key — it's the user's own history.
/// DataBlob holds all details encrypted (test type, result, notes).
/// TestedDate is plaintext for sorting without decryption.
/// </summary>
public class UserHealthStatus
{
    public int      Id          { get; set; }
    public string   DataBlob    { get; set; } = string.Empty;
    public DateTime TestedDate  { get; set; }
    public DateTime CreatedDate { get; set; }
}
