using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Vigma.PagosGateway.Infrastructure;
using Vigma.PagosGateway.Models;
using Vigma.PagosGateway.Utils;

namespace Vigma.PagosGateway.Services.Authentication
{
    /// <summary>
    /// Valida API Keys contra la base de datos de Vigma.TimbradoGateway
    /// Incluye caché para mejorar performance
    /// </summary>
    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly TimbradoDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ApiKeyValidator> _logger;
        
        private const string CACHE_KEY_PREFIX = "apikey:";
        private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(5);

        public ApiKeyValidator(
            TimbradoDbContext db,
            IMemoryCache cache,
            ILogger<ApiKeyValidator> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Valida un API Key (versión asíncrona)
        /// </summary>
        public async Task<ApiKeyValidationResult> ValidateAsync(string apiKey)
        {
            return await Task.Run(() => Validate(apiKey));
        }

        /// <summary>
        /// Valida un API Key (versión síncrona para middleware)
        /// </summary>
        public ApiKeyValidationResult Validate(string apiKey)
        {
            try
            {
                // 1. Validar formato básico
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogWarning("API Key vacía recibida");
                    return ApiKeyValidationResult.Failure("API Key no puede estar vacía");
                }

                if (!ApiKeyHelper.IsValidFormat(apiKey))
                {
                    _logger.LogWarning("API Key con formato inválido: {MaskedKey}", ApiKeyHelper.Mask(apiKey));
                    return ApiKeyValidationResult.Failure("Formato de API Key inválido");
                }

                // 2. Calcular hash
                string hash;
                try
                {
                    hash = ApiKeyHelper.Hash(apiKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al calcular hash de API Key");
                    return ApiKeyValidationResult.Failure("Error al procesar API Key");
                }

                // 3. Buscar en caché primero
                var cacheKey = $"{CACHE_KEY_PREFIX}{hash}";
                
                if (_cache.TryGetValue<Tenant>(cacheKey, out var cachedTenant) && cachedTenant != null)
                {
                    _logger.LogDebug("API Key encontrada en caché - Tenant: {TenantId}", cachedTenant.Id);
                    
                    // Verificar que siga activo (la caché puede tener datos obsoletos)
                    if (!cachedTenant.Activo)
                    {
                        _cache.Remove(cacheKey);
                        _logger.LogWarning("Tenant {TenantId} ya no está activo", cachedTenant.Id);
                        return ApiKeyValidationResult.Failure("Tenant inactivo");
                    }
                    
                    return ApiKeyValidationResult.Success(cachedTenant);
                }

                // 4. Buscar en base de datos
                var tenant = _db.Tenants
                    .AsNoTracking()
                    .FirstOrDefault(t => t.ApiKeyHash == hash);

                if (tenant == null)
                {
                    _logger.LogWarning("API Key no encontrada: {MaskedKey}", ApiKeyHelper.Mask(apiKey));
                    return ApiKeyValidationResult.Failure("API Key inválida");
                }

                // 5. Verificar que esté activo
                if (!tenant.Activo)
                {
                    _logger.LogWarning("Intento de uso de API Key inactiva - Tenant: {TenantId} ({TenantNombre})", 
                        tenant.Id, tenant.Nombre);
                    return ApiKeyValidationResult.Failure("Tenant inactivo. Contacte a soporte.");
                }

                // 6. Guardar en caché
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(CACHE_DURATION)
                    .SetPriority(CacheItemPriority.Normal);

                _cache.Set(cacheKey, tenant, cacheOptions);

                _logger.LogInformation(
                    "API Key validada exitosamente - Tenant: {TenantId} ({TenantNombre})",
                    tenant.Id,
                    tenant.Nombre);

                return ApiKeyValidationResult.Success(tenant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al validar API Key");
                return ApiKeyValidationResult.Failure("Error interno al validar API Key");
            }
        }

        /// <summary>
        /// Invalida la caché de un API Key específico
        /// Útil cuando se rota o desactiva un tenant
        /// </summary>
        public void InvalidateCache(string apiKey)
        {
            try
            {
                var hash = ApiKeyHelper.Hash(apiKey);
                var cacheKey = $"{CACHE_KEY_PREFIX}{hash}";
                _cache.Remove(cacheKey);
                
                _logger.LogInformation("Caché invalidada para API Key: {MaskedKey}", ApiKeyHelper.Mask(apiKey));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de API Key");
            }
        }

        /// <summary>
        /// Invalida toda la caché de API Keys
        /// Útil para refrescar después de cambios masivos
        /// </summary>
        public void InvalidateAllCache()
        {
            // Nota: IMemoryCache no tiene método Clear()
            // En producción, considera usar IDistributedCache (Redis)
            _logger.LogWarning("Se solicitó invalidar toda la caché de API Keys");
        }
    }
}
