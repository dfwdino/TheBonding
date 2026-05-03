namespace TheBonding.Domain.Entities;

/// <summary>
/// Single-row table. Inserted once during first-launch setup.
/// Holds all auth material needed to derive and verify Key1/Key2.
///
/// Auth flow:
///   1. User enters password.
///   2. PBKDF2(password, Salt) → Key1 (never stored).
///   3. SHA-256(Key1) compared to AuthVerifier → confirms correct password.
///   4. AES-256-CBC decrypt EncryptedKey2 with Key1 → Key2 in memory.
/// </summary>
public class AppSettings
{
    public int Id { get; set; }                    // always 1

    /// <summary>Base64-encoded PBKDF2 salt.</summary>
    public string Salt { get; set; } = string.Empty;

    /// <summary>Base64-encoded SHA-256(Key1). Used to verify the password is correct.</summary>
    public string AuthVerifier { get; set; } = string.Empty;

    /// <summary>Base64-encoded Key2 encrypted with Key1 (AES-256-CBC).</summary>
    public string EncryptedKey2 { get; set; } = string.Empty;

    public int FailedAttempts { get; set; }

    /// <summary>Null when not locked out.</summary>
    public DateTime? LockoutUntil { get; set; }

    /// <summary>False on first launch. True after setup is complete.</summary>
    public bool IsSetup { get; set; }
}
