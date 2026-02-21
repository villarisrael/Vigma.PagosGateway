using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Vigma.PagosGateway.Services.Authentication;
using Vigma.PagosGateway.Utils;

namespace Vigma.PagosGateway.Middleware
{
    /// <summary>
    /// Middleware que valida el API Key en cada request
    /// Compatible con el sistema de Vigma.TimbradoGateway
    /// </summary>
    public class ApiKeyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;
        
        // Nombres de headers aceptados para API Key
        private static readonly string[] API_KEY_HEADER_NAMES = new[]
        {
            "X-Api-Key",
            "X-API-KEY",
            "ApiKey"
        };

        public ApiKeyAuthenticationMiddleware(
            RequestDelegate next,
            ILogger<ApiKeyAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IApiKeyValidator apiKeyValidator)
        {
            // 1. Verificar si es un endpoint público (sin autenticación)
            if (IsPublicEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // 2. Extraer API Key del header o Authorization Bearer
            if (!TryGetApiKey(context.Request, out var apiKey))
            {
                _logger.LogWarning(
                    "Request sin API Key - Path: {Path}, IP: {IP}",
                    context.Request.Path,
                    context.Connection.RemoteIpAddress);

                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    ok = false,
                    error = "API Key requerida",
                    mensaje = "Debes incluir el header 'X-Api-Key' con tu API Key válida o usar 'Authorization: Bearer {apiKey}'",
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path.Value
                });
                return;
            }

            // 3. Validar API Key
            var validationResult = apiKeyValidator.Validate(apiKey);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning(
                    "API Key inválida - Key: {MaskedKey}, IP: {IP}, Path: {Path}, Razón: {Reason}",
                    ApiKeyHelper.Mask(apiKey),
                    context.Connection.RemoteIpAddress,
                    context.Request.Path,
                    validationResult.ErrorMessage);

                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    ok = false,
                    error = "API Key inválida",
                    mensaje = validationResult.ErrorMessage ?? "La API Key proporcionada no es válida o está inactiva",
                    timestamp = DateTime.UtcNow
                });
                return;
            }

            // 4. Guardar información del tenant en el contexto para uso posterior
            context.Items["TenantId"] = validationResult.TenantId;
            context.Items["TenantNombre"] = validationResult.TenantNombre;
            context.Items["ClientId"] = validationResult.ClientId;
            context.Items["ClientName"] = validationResult.ClientName;
            context.Items["PacUsuario"] = validationResult.PacUsuario;
            context.Items["PacProduccion"] = validationResult.PacProduccion;
            context.Items["AllowedGateways"] = validationResult.AllowedGateways;

            _logger.LogInformation(
                "Request autenticado - Tenant: {TenantNombre} (ID: {TenantId}), Path: {Path}",
                validationResult.TenantNombre,
                validationResult.TenantId,
                context.Request.Path);

            // 5. Continuar con el pipeline
            await _next(context);
        }

        /// <summary>
        /// Extrae el API Key del request (headers o Authorization Bearer)
        /// Compatible con el formato de Vigma.TimbradoGateway
        /// </summary>
        private bool TryGetApiKey(HttpRequest request, out string apiKey)
        {
            apiKey = string.Empty;

            // Opción 1: Headers X-Api-Key, X-API-KEY, ApiKey
            foreach (var headerName in API_KEY_HEADER_NAMES)
            {
                if (request.Headers.TryGetValue(headerName, out var headerValue) &&
                    !string.IsNullOrWhiteSpace(headerValue))
                {
                    apiKey = headerValue.ToString().Trim();
                    return true;
                }
            }

            // Opción 2: Authorization Bearer
            if (request.Headers.TryGetValue("Authorization", out var authHeader) &&
                !string.IsNullOrWhiteSpace(authHeader))
            {
                var authValue = authHeader.ToString();
                const string bearerPrefix = "Bearer ";
                
                if (authValue.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    apiKey = authValue.Substring(bearerPrefix.Length).Trim();
                    if (!string.IsNullOrWhiteSpace(apiKey))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determina si un endpoint es público (no requiere autenticación)
        /// </summary>
        private bool IsPublicEndpoint(PathString path)
        {
            var publicPaths = new[]
            {
                "/health",
                "/api/health",
                "/swagger",
                "/swagger/index.html",
                "/swagger/v1/swagger.json",
                "/_framework",
                "/_blazor"
            };

            var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;

            return publicPaths.Any(p => pathValue.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Extension method para registrar el middleware
    /// </summary>
    public static class ApiKeyAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyAuthenticationMiddleware>();
        }
    }
}
