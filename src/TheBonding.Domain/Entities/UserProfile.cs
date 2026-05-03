namespace TheBonding.Domain.Entities;

/// <summary>
/// Single-row table. Created on first profile save, not at setup.
/// DataBlob is an AES-256 encrypted JSON payload — see UserProfileRecord
/// in TheBonding.Shared for the decrypted structure.
/// </summary>
public class UserProfile
{
    public int Id { get; set; }                    // always 1

    /// <summary>Base64-encoded encrypted JSON blob.</summary>
    public string DataBlob { get; set; } = string.Empty;
}
