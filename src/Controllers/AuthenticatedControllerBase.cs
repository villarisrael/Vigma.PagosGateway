using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Vigma.PagosGateway.Controllers
{
    /// <summary>
    /// Controlador base con helpers para acceder a información del tenant autenticado
    /// </summary>
    public abstract class AuthenticatedControllerBase : ControllerBase
    {
        protected readonly ILogger Logger;

        protected AuthenticatedControllerBase(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Obtiene el ID del tenant autenticado
        /// </summary>
        protected int GetTenantId()
        {
            if (HttpContext.Items.TryGetValue("TenantId", out var tenantId) && tenantId is int id)
            {
                return id;
            }
            throw new UnauthorizedAccessException("No se pudo obtener el Tenant ID del contexto");
        }

        /// <summary>
        /// Obtiene el nombre del tenant autenticado
        /// </summary>
        protected string GetTenantNombre()
        {
            if (HttpContext.Items.TryGetValue("TenantNombre", out var nombre) && nombre is string name)
            {
                return name;
            }
            return "Desconocido";
        }

        /// <summary>
        /// Obtiene el usuario PAC del tenant
        /// </summary>
        protected string? GetPacUsuario()
        {
            if (HttpContext.Items.TryGetValue("PacUsuario", out var usuario) && usuario is string user)
            {
                return user;
            }
            return null;
        }

        /// <summary>
        /// Verifica si el tenant está en modo producción
        /// </summary>
        protected bool IsPacProduccion()
        {
            if (HttpContext.Items.TryGetValue("PacProduccion", out var produccion) && produccion is bool isProd)
            {
                return isProd;
            }
            return false;
        }

        /// <summary>
        /// Obtiene los gateways permitidos para este tenant
        /// </summary>
        protected List<string> GetAllowedGateways()
        {
            if (HttpContext.Items.TryGetValue("AllowedGateways", out var gateways) && gateways is List<string> list)
            {
                return list;
            }
            return new List<string> { "banamex", "banorte", "stripe" };
        }

        /// <summary>
        /// Verifica si el tenant tiene permiso para usar un gateway específico
        /// </summary>
        protected bool CanUseGateway(string gatewayName)
        {
            var allowed = GetAllowedGateways();
            return allowed.Contains(gatewayName.ToLowerInvariant());
        }

        /// <summary>
        /// Retorna un error 403 si el tenant no tiene permiso para el gateway
        /// </summary>
        protected IActionResult ForbiddenGateway(string gatewayName)
        {
            return StatusCode(403, new
            {
                ok = false,
                error = "Gateway no permitido",
                mensaje = $"Tu cuenta no tiene permisos para usar el gateway '{gatewayName}'",
                allowedGateways = GetAllowedGateways(),
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Log de información incluyendo datos del tenant
        /// </summary>
        protected void LogWithTenant(string message, params object[] args)
        {
            var tenantInfo = $"[Tenant: {GetTenantNombre()} ({GetTenantId()})] ";
            Logger.LogInformation(tenantInfo + message, args);
        }

        /// <summary>
        /// Log de error incluyendo datos del tenant
        /// </summary>
        protected void LogErrorWithTenant(Exception ex, string message, params object[] args)
        {
            var tenantInfo = $"[Tenant: {GetTenantNombre()} ({GetTenantId()})] ";
            Logger.LogError(ex, tenantInfo + message, args);
        }
    }
}
