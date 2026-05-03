namespace TheBonding.Application.Interfaces.Services;

/// <summary>
/// Handles all cryptographic operations for the app.
/// Implementation lives in TheBonding.Infrastructure using AES-256-CBC + PBKDF2-SHA256.
///
/// Key lifecycle:
///   Key1 = PBKDF2(password, salt, 600_000 iterations) — derived on unlock, never stored.
///   Key2 = random AES-256 key — generated at setup, stored encrypted by Key1.
///   All blob and string encryption uses Key2.
/// </summary>
public interface IEncryptionService
{
    // ------------------------------------------------------------------
    // First-launch setup
    // ------------------------------------------------------------------

    /// <summary>Generates a new cryptographically random PBKDF2 salt.</summary>
    byte[] GenerateSalt();

    /// <summary>Generates a new random AES-256 Key2.</summary>
    byte[] GenerateKey2();

    /// <summary>Derives Key1 from the user's password and the stored salt.</summary>
    byte[] DeriveKey1(string password, byte[] salt);

    /// <summary>
    /// Computes the AuthVerifier stored in AppSettings.
    /// AuthVerifier = Base64(SHA-256(Key1)).
    /// Used to verify a password attempt is correct without storing Key1.
    /// </summary>
    string ComputeAuthVerifier(byte[] key1);

    /// <summary>Encrypts Key2 with Key1. Returns Base64 ciphertext stored in AppSettings.</summary>
    string EncryptKey2(byte[] key2, byte[] key1);

    /// <summary>Decrypts Key2 using Key1. Returns the raw Key2 bytes held in memory.</summary>
    byte[] DecryptKey2(string encryptedKey2Base64, byte[] key1);

    // ------------------------------------------------------------------
    // Blob encryption — used for DataBlob columns
    // ------------------------------------------------------------------

    /// <summary>Serializes a record to JSON, then encrypts it with Key2. Returns Base64 ciphertext.</summary>
    string EncryptBlob<T>(T record, byte[] key2) where T : class;

    /// <summary>Decrypts a Base64 ciphertext with Key2, then deserializes to T. Returns null on failure.</summary>
    T? DecryptBlob<T>(string encryptedBlobBase64, byte[] key2) where T : class;

    // ------------------------------------------------------------------
    // String encryption — used for LookupItem.EncryptedValue
    // ------------------------------------------------------------------

    /// <summary>Encrypts a plain string with Key2. Returns Base64 ciphertext.</summary>
    string EncryptString(string value, byte[] key2);

    /// <summary>Decrypts a Base64 ciphertext with Key2. Returns the original string.</summary>
    string DecryptString(string encryptedValueBase64, byte[] key2);
}
