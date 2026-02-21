namespace Vigma.PagosGateway.Services.Authentication
{
    /// <summary>
    /// Interfaz para validar API Keys
    /// </summary>
    public interface IApiKeyValidator
    {
        /// <summary>
        /// Valida un API Key contra la base de datos de tenants
        /// </summary>
        /// <param name="apiKey">El API Key a validar</param>
        /// <returns>Resultado de la validación con información del tenant</returns>
        Task<ApiKeyValidationResult> ValidateAsync(string apiKey);

        /// <summary>
        /// Valida un API Key de forma síncrona (para middleware)
        /// </summary>
        ApiKeyValidationResult Validate(string apiKey);
    }
}
