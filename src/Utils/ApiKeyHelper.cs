using System.Security.Cryptography;
using System.Text;

namespace Vigma.PagosGateway.Utils
{
    /// <summary>
    /// Utilidades para generar y validar API Keys
    /// Compatible con Vigma.TimbradoGateway
    /// </summary>
    public static class ApiKeyHelper
    {
        /// <summary>
        /// Genera el hash SHA256 de un API Key
        /// Debe coincidir con el hash almacenado en la BD
        /// </summary>
        public static string Hash(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API Key no puede estar vacía", nameof(apiKey));

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(apiKey);
            var hashBytes = sha256.ComputeHash(bytes);
            
            // Convertir a hexadecimal (64 caracteres)
            return BitConverter.ToString(hashBytes)
                .Replace("-", "")
                .ToLowerInvariant();
        }

        /// <summary>
        /// Obtiene los últimos 4 caracteres del API Key
        /// </summary>
        public static string Last4(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return string.Empty;

            return apiKey.Length >= 4
                ? apiKey.Substring(apiKey.Length - 4)
                : apiKey;
        }

        /// <summary>
        /// Enmascara el API Key para logs (muestra solo los últimos 4)
        /// Ejemplo: "****-****-****-1234"
        /// </summary>
        public static string Mask(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return "****";

            if (apiKey.Length <= 4)
                return new string('*', apiKey.Length);

            var last4 = Last4(apiKey);
            var masked = new string('*', Math.Min(apiKey.Length - 4, 12));
            
            return $"{masked}{last4}";
        }

        /// <summary>
        /// Valida que el formato del API Key sea correcto
        /// </summary>
        public static bool IsValidFormat(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return false;

            // Debe tener al menos 32 caracteres (ajustar según tu formato)
            if (apiKey.Length < 32)
                return false;

            // Debe contener solo caracteres alfanuméricos, guiones y guiones bajos
            return apiKey.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
        }
    }
}
