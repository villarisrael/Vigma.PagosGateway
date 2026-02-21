using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using Vigma.PagosGateway.Services.Logging;
using Vigma.PagosGateway.Utils;

namespace Vigma.PagosGateway.Middleware
{
    /// <summary>
    /// Middleware que registra automáticamente todos los intentos de pago
    /// </summary>
    public class PagoLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PagoLoggingMiddleware> _logger;

        public PagoLoggingMiddleware(
            RequestDelegate next,
            ILogger<PagoLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IPagoLoggerService pagoLogger)
        {
            // Solo registrar endpoints de pago
            if (!IsPaymentEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var tenantId = GetTenantId(context);
            var gateway = ExtractGateway(context.Request.Path);
            var ipCliente = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var apiKeyLast4 = context.Items["ClientId"]?.ToString();

            try
            {
                // Ejecutar el request
                await _next(context);

                stopwatch.Stop();

                // Registrar el intento
                if (tenantId.HasValue)
                {
                    var intentoData = new PagoIntentoData
                    {
                        TenantId = tenantId.Value,
                        Gateway = gateway,
                        Ambiente = IsProdEnvironment(context) ? "produccion" : "test",
                        Endpoint = context.Request.Path,
                        MetodoHttp = context.Request.Method,
                        TipoOperacion = ExtractOperation(context.Request.Path),
                        IpCliente = ipCliente,
                        UserAgent = userAgent,
                        ApiKeyLast4 = apiKeyLast4,
                        Exitoso = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300,
                        HttpStatusCode = context.Response.StatusCode,
                        DuracionMs = (int)stopwatch.ElapsedMilliseconds
                    };

                    // Ejecutar el logging de forma asíncrona sin bloquear
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await pagoLogger.LogIntentoAsync(intentoData);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error al registrar intento de pago");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Registrar el error
                if (tenantId.HasValue)
                {
                    var intentoData = new PagoIntentoData
                    {
                        TenantId = tenantId.Value,
                        Gateway = gateway,
                        Ambiente = IsProdEnvironment(context) ? "produccion" : "test",
                        Endpoint = context.Request.Path,
                        MetodoHttp = context.Request.Method,
                        TipoOperacion = ExtractOperation(context.Request.Path),
                        IpCliente = ipCliente,
                        UserAgent = userAgent,
                        ApiKeyLast4 = apiKeyLast4,
                        Exitoso = false,
                        HttpStatusCode = 500,
                        ErrorTipo = ex.GetType().Name,
                        DuracionMs = (int)stopwatch.ElapsedMilliseconds
                    };

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await pagoLogger.LogIntentoAsync(intentoData);
                        }
                        catch { }
                    });
                }

                throw;
            }
        }

        private bool IsPaymentEndpoint(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;
            
            return pathValue.Contains("/api/banamex/") ||
                   pathValue.Contains("/api/banorte/") ||
                   pathValue.Contains("/api/stripe/") ||
                   pathValue.Contains("/pay") ||
                   pathValue.Contains("/charge") ||
                   pathValue.Contains("/process");
        }

        private int? GetTenantId(HttpContext context)
        {
            if (context.Items.TryGetValue("TenantId", out var tenantId) && tenantId is int id)
            {
                return id;
            }
            return null;
        }

        private string ExtractGateway(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;
            
            if (pathValue.Contains("/banamex")) return "banamex";
            if (pathValue.Contains("/banorte")) return "banorte";
            if (pathValue.Contains("/stripe")) return "stripe";
            
            return "unknown";
        }

        private string ExtractOperation(PathString path)
        {
            var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;
            
            if (pathValue.Contains("/pay")) return "PAY";
            if (pathValue.Contains("/authorize")) return "AUTHORIZE";
            if (pathValue.Contains("/capture")) return "CAPTURE";
            if (pathValue.Contains("/refund")) return "REFUND";
            if (pathValue.Contains("/void")) return "VOID";
            if (pathValue.Contains("/charge")) return "PAY";
            
            return "UNKNOWN";
        }

        private bool IsProdEnvironment(HttpContext context)
        {
            if (context.Items.TryGetValue("PacProduccion", out var isProd) && isProd is bool prod)
            {
                return prod;
            }
            return false;
        }
    }

    public static class PagoLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UsePagoLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PagoLoggingMiddleware>();
        }
    }
}
