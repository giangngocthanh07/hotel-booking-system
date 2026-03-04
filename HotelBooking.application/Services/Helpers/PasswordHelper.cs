using BCrypt.Net;

namespace HotelBooking.application.Helpers;

public static class PasswordHelper
{
    /// <summary>
    /// Hashes a password using BCrypt with automatic salt generation
    /// </summary>
    /// <param name="password">The password to hash</param>
    /// <param name="workFactor">Complexity factor (default is 12)</param>
    /// <returns>The hashed password string</returns>
    public static string HashPassword(string password, int workFactor = 12)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password must not be empty", nameof(password));
        }

        if (workFactor < 4 || workFactor > 31)
        {
            throw new ArgumentException("Work factor must be between 4 and 31", nameof(workFactor));
        }

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
    }

    /// <summary>
    /// Verifies a password against a stored hash
    /// </summary>
    /// <param name="password">The password to verify</param>
    /// <param name="hashedPassword">The stored hash string</param>
    /// <returns>True if the password matches, False otherwise</returns>
    public static bool VerifyPassword(string password = "", string hashedPassword = "")
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Password must not be empty", nameof(password));
        }

        if (string.IsNullOrEmpty(hashedPassword))
        {
            throw new ArgumentException("Hash must not be empty", nameof(hashedPassword));
        }

        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}