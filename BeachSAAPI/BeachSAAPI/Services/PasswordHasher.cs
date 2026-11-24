using System.Security.Cryptography;
using System.Text;

namespace BeachSAAPI.Services
{
    /// <summary>
    /// Cifra y verifica contraseñas con SHA-256.
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Calcula el hash SHA-256 de una contraseña.
        /// </summary>
        /// <param name="password">Contraseña en texto plano.</param>
        /// <returns>El hash en hexadecimal, de 64 caracteres.</returns>
        public static string Hash(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password ?? ""));
            return Convert.ToHexString(bytes);
        }

        /// <summary>
        /// Indica si un texto ya es un hash SHA-256.
        /// </summary>
        /// <param name="value">Texto a buscar.</param>
        /// <returns>true si tiene 64 caracteres hexadecimales.</returns>
        public static bool IsHash(string value)
        {
            return value != null && value.Length == 64 && value.All(Uri.IsHexDigit);
        }

        /// <summary>
        /// Compara una contraseña con la guardada, sea hash o texto plano.
        /// </summary>
        /// <param name="password">Contraseña en texto plano.</param>
        /// <param name="stored">Contraseña guardada en la base de datos.</param>
        /// <returns>true si la contraseña coincide.</returns>
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
