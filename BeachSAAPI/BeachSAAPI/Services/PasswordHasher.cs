using System.Security.Cryptography;
using System.Text;

namespace BeachSAAPI.Services
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password ?? ""));
            return Convert.ToHexString(bytes);
        }

        public static bool IsHash(string value)
        {
            return value != null && value.Length == 64 && value.All(Uri.IsHexDigit);
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored))
            {
                return false;
            }

            return IsHash(stored) ? stored == Hash(password) : stored == password;
        }
    }
}
