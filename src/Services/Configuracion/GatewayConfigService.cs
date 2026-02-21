using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Vigma.PagosGateway.Infrastructure;
using Vigma.PagosGateway.Models;
using Vigma.PagosGateway.Models.GatewayConfigs;


namespace Vigma.PagosGateway.Services.Configuration
{
    /// <summary>
    /// Interfaz para obtener configuraciones de gateway por tenant
    /// </summary>
    public interface IGatewayConfigService
    {
        Task<BanamexTenantConfig?> GetBanamexConfigAsync(int tenantId, string ambiente = "test");
        Task<BanorteTenantConfig?> GetBanorteConfigAsync(int tenantId, string ambiente = "test");
        Task<StripeTenantConfig?> GetStripeConfigAsync(int tenantId, string ambiente = "test");
        Task<bool> TenantHasGatewayAsync(int tenantId, string gateway, string ambiente = "test");
        Task<List<string>> GetAvailableGatewaysAsync(int tenantId, string ambiente = "test");
    }

    /// <summary>
    /// Servicio para obtener configuraciones de gateway por tenant
    /// </summary>
    public class GatewayConfigService : IGatewayConfigService
    {
        private readonly TimbradoDbContext _db;
        private readonly CryptoService _crypto;
        private readonly ILogger<GatewayConfigService> _logger;

        public GatewayConfigService(
            TimbradoDbContext db,
            CryptoService crypto,
            ILogger<GatewayConfigService> logger)
        {
            _db = db;
            _crypto = crypto;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la configuración de Banamex para un tenant
        /// </summary>
        public async Task<BanamexTenantConfig?> GetBanamexConfigAsync(int tenantId, string ambiente = "test")
        {
            try
            {
                var config = await _db.TenantGatewayConfigs
                    .FirstOrDefaultAsync(c =>
                        c.TenantId == tenantId &&
                        c.Gateway == "banamex" &&
                        c.Ambiente == ambiente &&
                        c.Activo);

                if (config == null)
                {
                    _logger.LogWarning("Tenant {TenantId} no tiene configuración de Banamex en {Ambiente}", 
                        tenantId, ambiente);
                    return null;
                }

                // Desencriptar y deserializar
                var configJson = _crypto.DecryptFromBase64(config.ConfigJsonEnc);
                var banamexConfig = JsonConvert.DeserializeObject<BanamexTenantConfig>(configJson);

                _logger.LogDebug("Configuración de Banamex obtenida para tenant {TenantId}", tenantId);
                
                return banamexConfig;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración de Banamex para tenant {TenantId}", tenantId);
                return null;
            }
        }

        /// <summary>
        /// Obtiene la configuración de Banorte para un tenant
        /// </summary>
        public async Task<BanorteTenantConfig?> GetBanorteConfigAsync(int tenantId, string ambiente = "test")
        {
            try
            {
                var config = await _db.TenantGatewayConfigs
                    .FirstOrDefaultAsync(c =>
                        c.TenantId == tenantId &&
                        c.Gateway == "banorte" &&
                        c.Ambiente == ambiente &&
                        c.Activo);

                if (config == null)
                {
                    _logger.LogWarning("Tenant {TenantId} no tiene configuración de Banorte en {Ambiente}", 
                        tenantId, ambiente);
                    return null;
                }

                var configJson = _crypto.DecryptFromBase64(config.ConfigJsonEnc);
                var banorteConfig = JsonConvert.DeserializeObject<BanorteTenantConfig>(configJson);

                return banorteConfig;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración de Banorte para tenant {TenantId}", tenantId);
                return null;
            }
        }

        /// <summary>
        /// Obtiene la configuración de Stripe para un tenant
        /// </summary>
        public async Task<StripeTenantConfig?> GetStripeConfigAsync(int tenantId, string ambiente = "test")
        {
            try
            {
                var config = await _db.TenantGatewayConfigs
                    .FirstOrDefaultAsync(c =>
                        c.TenantId == tenantId &&
                        c.Gateway == "stripe" &&
                        c.Ambiente == ambiente &&
                        c.Activo);

                if (config == null)
                {
                    _logger.LogWarning("Tenant {TenantId} no tiene configuración de Stripe en {Ambiente}", 
                        tenantId, ambiente);
                    return null;
                }

                var configJson = _crypto.DecryptFromBase64(config.ConfigJsonEnc);
                var stripeConfig = JsonConvert.DeserializeObject<StripeTenantConfig>(configJson);

                return stripeConfig;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración de Stripe para tenant {TenantId}", tenantId);
                return null;
            }
        }

        /// <summary>
        /// Verifica si un tenant tiene un gateway configurado
        /// </summary>
        public async Task<bool> TenantHasGatewayAsync(int tenantId, string gateway, string ambiente = "test")
        {
            return await _db.TenantGatewayConfigs
                .AnyAsync(c =>
                    c.TenantId == tenantId &&
                    c.Gateway == gateway.ToLowerInvariant() &&
                    c.Ambiente == ambiente &&
                    c.Activo);
        }

        /// <summary>
        /// Obtiene la lista de gateways disponibles para un tenant
        /// </summary>
        public async Task<List<string>> GetAvailableGatewaysAsync(int tenantId, string ambiente = "test")
        {
            return await _db.TenantGatewayConfigs
                .Where(c =>
                    c.TenantId == tenantId &&
                    c.Ambiente == ambiente &&
                    c.Activo)
                .Select(c => c.Gateway)
                .ToListAsync();
        }
    }

    /// <summary>
    /// Servicio de cifrado/descifrado (usa el mismo que TimbradoGateway)
    /// </summary>
    public class CryptoService
    {
        private readonly IConfiguration _config;

        public CryptoService(IConfiguration config)
        {
            _config = config;
        }

        public string EncryptToBase64(string plainText)
        {
            // TODO: Implementar cifrado real (AES-256)
            // Por ahora, solo base64 (NO seguro para producción)
            var bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(bytes);
        }

        public string DecryptFromBase64(string encryptedBase64)
        {
            // TODO: Implementar descifrado real (AES-256)
            // Por ahora, solo base64
            var bytes = Convert.FromBase64String(encryptedBase64);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
