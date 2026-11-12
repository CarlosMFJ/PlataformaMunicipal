using System.Security.Cryptography;

namespace PlataformaMunicipal.Helpers
{
    public static class PasswordHelper
    {
        public static byte[] Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
            return salt.Concat(hash).ToArray();
        }

        public static bool Verify(string password, byte[] stored)
        {
            if (stored == null || stored.Length != 48) return false;
            var salt = stored[..16];
            var expected = stored[16..];
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
    }
}