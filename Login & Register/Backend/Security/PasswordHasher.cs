using System.Security.Cryptography;

namespace PennywiseAuth.Security;

/// <summary>
/// Provides secure password hashing and verification using PBKDF2 (HMAC-SHA256).
/// Follows OWASP recommendations with 100,000 iterations, 32-byte salt, and constant-time verification.
/// </summary>
public static class PasswordHasher
{
    private const int SaltSizeBytes = 32;       // 256-bit salt
    private const int HashSizeBytes = 32;       // 256-bit subkey
    private const int Iterations = 100_000;     // OWASP recommended minimum for SHA-256
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    /// <summary>
    /// Generates a cryptographically secure random salt and computes the PBKDF2 hash of the given plaintext password.
    /// </summary>
    /// <param name="password">Plaintext password to hash.</param>
    /// <param name="salt">Output Base64-encoded random salt.</param>
    /// <returns>Base64-encoded password hash.</returns>
    public static string HashPassword(string password, out string salt)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        salt = Convert.ToBase64String(saltBytes);

        byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            Algorithm,
            HashSizeBytes);

        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Verifies that a plaintext password matches the stored PBKDF2 hash using constant-time comparison.
    /// </summary>
    /// <param name="password">Plaintext password provided during login.</param>
    /// <param name="storedHash">Base64-encoded hash from the database.</param>
    /// <param name="storedSalt">Base64-encoded salt from the database.</param>
    /// <returns>True if password matches; otherwise false.</returns>
    public static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash) || string.IsNullOrWhiteSpace(storedSalt))
            return false;

        try
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            byte[] expectedHashBytes = Convert.FromBase64String(storedHash);

            byte[] actualHashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                Iterations,
                Algorithm,
                HashSizeBytes);

            // Use constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(actualHashBytes, expectedHashBytes);
        }
        catch
        {
            return false;
        }
    }
}
