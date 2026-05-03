using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheBonding.Application.Interfaces.Services;

namespace TheBonding.Infrastructure.Services;

/// <summary>
/// AES-256-CBC encryption with PBKDF2-SHA256 key derivation.
///
/// Encrypted format: Base64(IV[16 bytes] + Ciphertext)
/// IV is randomly generated per encryption — never reused.
///
/// Key derivation: PBKDF2-SHA256, 600,000 iterations (NIST SP 800-132 recommendation).
/// AuthVerifier: Base64(SHA-256(Key1)) — proves password correctness without storing Key1.
/// </summary>
public sealed class EncryptionService : IEncryptionService
{
    private const int SaltSize       = 32;       // 256-bit salt
    private const int KeySize        = 32;       // 256-bit key
    private const int IvSize         = 16;       // 128-bit AES block size
    private const int Iterations     = 600_000;  // PBKDF2 iterations

    // -------------------------------------------------------------------------
    // Key material generation and derivation
    // -------------------------------------------------------------------------

    public byte[] GenerateSalt() =>
        RandomNumberGenerator.GetBytes(SaltSize);

    public byte[] GenerateKey2() =>
        RandomNumberGenerator.GetBytes(KeySize);

    public byte[] DeriveKey1(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

    public string ComputeAuthVerifier(byte[] key1) =>
        Convert.ToBase64String(SHA256.HashData(key1));

    public string EncryptKey2(byte[] key2, byte[] key1) =>
        Encrypt(key2, key1);

    public byte[] DecryptKey2(string encryptedKey2Base64, byte[] key1) =>
        Decrypt(encryptedKey2Base64, key1);

    // -------------------------------------------------------------------------
    // Blob encryption — JSON + AES-256-CBC
    // -------------------------------------------------------------------------

    public string EncryptBlob<T>(T record, byte[] key2) where T : class
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(record);
        return Encrypt(json, key2);
    }

    public T? DecryptBlob<T>(string encryptedBlobBase64, byte[] key2) where T : class
    {
        try
        {
            var json = Decrypt(encryptedBlobBase64, key2);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return null;
        }
    }

    // -------------------------------------------------------------------------
    // String encryption — UTF-8 + AES-256-CBC
    // -------------------------------------------------------------------------

    public string EncryptString(string value, byte[] key2) =>
        Encrypt(Encoding.UTF8.GetBytes(value), key2);

    public string DecryptString(string encryptedValueBase64, byte[] key2) =>
        Encoding.UTF8.GetString(Decrypt(encryptedValueBase64, key2));

    // -------------------------------------------------------------------------
    // Core AES-256-CBC encrypt / decrypt
    // -------------------------------------------------------------------------

    private static string Encrypt(byte[] data, byte[] key)
    {
        using var aes = Aes.Create();
        aes.Key     = key;
        aes.Mode    = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var ciphertext = encryptor.TransformFinalBlock(data, 0, data.Length);

        // Output: IV (16 bytes) || Ciphertext
        var result = new byte[IvSize + ciphertext.Length];
        aes.IV.CopyTo(result, 0);
        ciphertext.CopyTo(result, IvSize);

        return Convert.ToBase64String(result);
    }

    private static byte[] Decrypt(string base64, byte[] key)
    {
        var combined  = Convert.FromBase64String(base64);
        var iv        = combined[..IvSize];
        var ciphertext = combined[IvSize..];

        using var aes = Aes.Create();
        aes.Key     = key;
        aes.IV      = iv;
        aes.Mode    = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
    }
}
