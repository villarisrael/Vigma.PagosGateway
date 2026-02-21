using Vigma.PagosGateway.Models;

namespace Vigma.PagosGateway.Services.Authentication
{
    /// <summary>
    /// Resultado de la validación de un API Key
    /// </summary>
    public class ApiKeyValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        
        // Información del tenant validado
        public int TenantId { get; set; }
        public string? TenantNombre { get; set; }
        public string? PacUsuario { get; set; }
        public bool PacProduccion { get; set; }
        
        // Para uso en el contexto HTTP
        public string ClientId => TenantId.ToString();
        public string? ClientName => TenantNombre;
        public string? ClientEmail => PacUsuario;
        
        // Gateways permitidos (para futuro control de permisos)
        public List<string> AllowedGateways { get; set; } = new List<string>
        {
            "banamex",
            "banorte",
            "stripe"
        };

        public static ApiKeyValidationResult Success(Tenant tenant)
        {
            return new ApiKeyValidationResult
            {
                IsValid = true,
                TenantId = tenant.Id,
                TenantNombre = tenant.Nombre,
                PacUsuario = tenant.PacUsuario,
                PacProduccion = tenant.PacProduccion
            };
        }

        public static ApiKeyValidationResult Failure(string errorMessage)
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
