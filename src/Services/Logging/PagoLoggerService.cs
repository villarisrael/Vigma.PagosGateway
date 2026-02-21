using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using Vigma.PagosGateway.Infrastructure;
using Vigma.PagosGateway.Models.Logging;

namespace Vigma.PagosGateway.Services.Logging
{
    /// <summary>
    /// Servicio para registrar todas las operaciones de pago
    /// </summary>
    public interface IPagoLoggerService
    {
        Task LogExitosoAsync(PagoExitosoData data);
        Task LogErrorAsync(PagoErrorData data);
        Task LogIntentoAsync(PagoIntentoData data);
        Task<PagoOkLog?> GetTransactionAsync(string orderId, string transactionId);
        Task<List<PagoOkLog>> GetTenantTransactionsAsync(int tenantId, DateTime desde, DateTime hasta);
        Task<Dictionary<string, object>> GetStatsAsync(int tenantId, int dias = 30);
    }

    public class PagoLoggerService : IPagoLoggerService
    {
        private readonly TimbradoDbContext _db;
        private readonly ILogger<PagoLoggerService> _logger;

        public PagoLoggerService(
            TimbradoDbContext db,
            ILogger<PagoLoggerService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Registra una transacción exitosa
        /// </summary>
        public async Task LogExitosoAsync(PagoExitosoData data)
        {
            try
            {
                var log = new PagoOkLog
                {
                    TenantId = data.TenantId,
                    Gateway = data.Gateway,
                    Ambiente = data.Ambiente,
                    OrderId = data.OrderId,
                    TransactionId = data.TransactionId,
                    GatewayTransactionId = data.GatewayTransactionId,
                    TipoOperacion = data.TipoOperacion,
                    Monto = data.Monto,
                    Moneda = data.Moneda,
                    Descripcion = data.Descripcion,
                    TarjetaTipo = data.TarjetaTipo,
                    TarjetaLast4 = data.TarjetaLast4,
                    TarjetaBin = data.TarjetaBin,
                    GatewayCode = data.GatewayCode,
                    GatewayMessage = data.GatewayMessage,
                    ResultCode = data.ResultCode,
                    IpCliente = data.IpCliente,
                    UserAgent = data.UserAgent,
                    DuracionMs = data.DuracionMs,
                    Servidor = Environment.MachineName,
                    RequestJson = data.RequestJson,
                    ResponseJson = data.ResponseJson,
                    MetadataJson = data.MetadataJson,
                    CreadoUtc = DateTime.UtcNow
                };

                _db.PagosOkLog.Add(log);
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "✅ Pago exitoso registrado - Tenant: {TenantId}, Gateway: {Gateway}, Order: {OrderId}, Monto: {Monto} {Moneda}",
                    data.TenantId, data.Gateway, data.OrderId, data.Monto, data.Moneda);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar pago exitoso - Order: {OrderId}", data.OrderId);
                // No propagamos el error para no afectar el flujo principal
            }
        }

        /// <summary>
        /// Registra un error en el pago
        /// </summary>
        public async Task LogErrorAsync(PagoErrorData data)
        {
            try
            {
                var log = new PagoErrorLog
                {
                    TenantId = data.TenantId,
                    Gateway = data.Gateway,
                    Ambiente = data.Ambiente,
                    OrderId = data.OrderId,
                    TransactionId = data.TransactionId,
                    TipoOperacion = data.TipoOperacion,
                    Monto = data.Monto,
                    Moneda = data.Moneda,
                    ErrorTipo = data.ErrorTipo,
                    ErrorCodigo = data.ErrorCodigo,
                    ErrorMensaje = data.ErrorMensaje,
                    ErrorDetalle = data.ErrorDetalle,
                    TarjetaTipo = data.TarjetaTipo,
                    TarjetaLast4 = data.TarjetaLast4,
                    TarjetaBin = data.TarjetaBin,
                    GatewayCode = data.GatewayCode,
                    GatewayMessage = data.GatewayMessage,
                    HttpStatusCode = data.HttpStatusCode,
                    IpCliente = data.IpCliente,
                    UserAgent = data.UserAgent,
                    DuracionMs = data.DuracionMs,
                    Servidor = Environment.MachineName,
                    RequestJson = data.RequestJson,
                    ResponseJson = data.ResponseJson,
                    StackTrace = data.StackTrace,
                    MetadataJson = data.MetadataJson,
                    IntentoNumero = data.IntentoNumero,
                    Reintentar = data.Reintentar,
                    CreadoUtc = DateTime.UtcNow
                };

                _db.PagosErrorLog.Add(log);
                await _db.SaveChangesAsync();

                _logger.LogWarning(
                    "❌ Error de pago registrado - Tenant: {TenantId}, Gateway: {Gateway}, Tipo: {ErrorTipo}, Código: {ErrorCodigo}",
                    data.TenantId, data.Gateway, data.ErrorTipo, data.ErrorCodigo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar error de pago - Order: {OrderId}", data.OrderId);
            }
        }

        /// <summary>
        /// Registra un intento de pago (exitoso o no)
        /// </summary>
        public async Task LogIntentoAsync(PagoIntentoData data)
        {
            try
            {
                var log = new PagoIntentoLog
                {
                    TenantId = data.TenantId,
                    Gateway = data.Gateway,
                    Ambiente = data.Ambiente,
                    Endpoint = data.Endpoint,
                    MetodoHttp = data.MetodoHttp,
                    TipoOperacion = data.TipoOperacion,
                    OrderId = data.OrderId,
                    TransactionId = data.TransactionId,
                    IpCliente = data.IpCliente,
                    UserAgent = data.UserAgent,
                    ApiKeyLast4 = data.ApiKeyLast4,
                    TarjetaLast4 = data.TarjetaLast4,
                    TarjetaBin = data.TarjetaBin,
                    Exitoso = data.Exitoso,
                    HttpStatusCode = data.HttpStatusCode,
                    ErrorTipo = data.ErrorTipo,
                    Monto = data.Monto,
                    Moneda = data.Moneda,
                    DuracionMs = data.DuracionMs,
                    MetadataJson = data.MetadataJson,
                    CreadoUtc = DateTime.UtcNow
                };

                _db.PagosIntentoLog.Add(log);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar intento de pago");
            }
        }

        /// <summary>
        /// Obtiene una transacción específica
        /// </summary>
        public async Task<PagoOkLog?> GetTransactionAsync(string orderId, string transactionId)
        {
            return await _db.PagosOkLog
                .FirstOrDefaultAsync(p => 
                    p.OrderId == orderId && 
                    p.TransactionId == transactionId);
        }

        /// <summary>
        /// Obtiene transacciones de un tenant en un rango de fechas
        /// </summary>
        public async Task<List<PagoOkLog>> GetTenantTransactionsAsync(
            int tenantId, 
            DateTime desde, 
            DateTime hasta)
        {
            return await _db.PagosOkLog
                .Where(p => 
                    p.TenantId == tenantId &&
                    p.CreadoUtc >= desde &&
                    p.CreadoUtc <= hasta)
                .OrderByDescending(p => p.CreadoUtc)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene estadísticas de un tenant
        /// </summary>
        public async Task<Dictionary<string, object>> GetStatsAsync(int tenantId, int dias = 30)
        {
            var desde = DateTime.UtcNow.AddDays(-dias);

            var exitosos = await _db.PagosOkLog
                .Where(p => p.TenantId == tenantId && p.CreadoUtc >= desde)
                .ToListAsync();

            var errores = await _db.PagosErrorLog
                .Where(p => p.TenantId == tenantId && p.CreadoUtc >= desde)
                .ToListAsync();

            var total = exitosos.Count + errores.Count;
            var tasaExito = total > 0 ? (decimal)exitosos.Count / total * 100 : 0;

            return new Dictionary<string, object>
            {
                { "periodo_dias", dias },
                { "total_transacciones", total },
                { "exitosos", exitosos.Count },
                { "errores", errores.Count },
                { "tasa_exito_pct", Math.Round(tasaExito, 2) },
                { "monto_total", exitosos.Sum(p => p.Monto) },
                { "monto_promedio", exitosos.Any() ? exitosos.Average(p => p.Monto) : 0 },
                { "duracion_promedio_ms", exitosos.Any() ? exitosos.Average(p => p.DuracionMs ?? 0) : 0 },
                { "por_gateway", exitosos.GroupBy(p => p.Gateway)
                    .Select(g => new { 
                        gateway = g.Key, 
                        cantidad = g.Count(), 
                        monto = g.Sum(p => p.Monto) 
                    })
                    .ToList() },
                { "top_errores", errores.GroupBy(p => new { p.ErrorTipo, p.ErrorCodigo })
                    .Select(g => new { 
                        tipo = g.Key.ErrorTipo, 
                        codigo = g.Key.ErrorCodigo, 
                        cantidad = g.Count() 
                    })
                    .OrderByDescending(x => x.cantidad)
                    .Take(10)
                    .ToList() }
            };
        }
    }

    #region Data Transfer Objects

    public class PagoExitosoData
    {
        public int TenantId { get; set; }
        public string Gateway { get; set; } = string.Empty;
        public string Ambiente { get; set; } = "test";
        public string OrderId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string? GatewayTransactionId { get; set; }
        public string TipoOperacion { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Moneda { get; set; } = "MXN";
        public string? Descripcion { get; set; }
        public string? TarjetaTipo { get; set; }
        public string? TarjetaLast4 { get; set; }
        public string? TarjetaBin { get; set; }
        public string? GatewayCode { get; set; }
        public string? GatewayMessage { get; set; }
        public string? ResultCode { get; set; }
        public string? IpCliente { get; set; }
        public string? UserAgent { get; set; }
        public int? DuracionMs { get; set; }
        public string? RequestJson { get; set; }
        public string? ResponseJson { get; set; }
        public string? MetadataJson { get; set; }
    }

    public class PagoErrorData
    {
        public int TenantId { get; set; }
        public string Gateway { get; set; } = string.Empty;
        public string Ambiente { get; set; } = "test";
        public string? OrderId { get; set; }
        public string? TransactionId { get; set; }
        public string TipoOperacion { get; set; } = string.Empty;
        public decimal? Monto { get; set; }
        public string Moneda { get; set; } = "MXN";
        public string ErrorTipo { get; set; } = string.Empty;
        public string? ErrorCodigo { get; set; }
        public string? ErrorMensaje { get; set; }
        public string? ErrorDetalle { get; set; }
        public string? TarjetaTipo { get; set; }
        public string? TarjetaLast4 { get; set; }
        public string? TarjetaBin { get; set; }
        public string? GatewayCode { get; set; }
        public string? GatewayMessage { get; set; }
        public int? HttpStatusCode { get; set; }
        public string? IpCliente { get; set; }
        public string? UserAgent { get; set; }
        public int? DuracionMs { get; set; }
        public string? RequestJson { get; set; }
        public string? ResponseJson { get; set; }
        public string? StackTrace { get; set; }
        public string? MetadataJson { get; set; }
        public int IntentoNumero { get; set; } = 1;
        public bool Reintentar { get; set; } = false;
    }

    public class PagoIntentoData
    {
        public int TenantId { get; set; }
        public string Gateway { get; set; } = string.Empty;
        public string Ambiente { get; set; } = "test";
        public string Endpoint { get; set; } = string.Empty;
        public string MetodoHttp { get; set; } = "POST";
        public string TipoOperacion { get; set; } = string.Empty;
        public string? OrderId { get; set; }
        public string? TransactionId { get; set; }
        public string IpCliente { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string? ApiKeyLast4 { get; set; }
        public string? TarjetaLast4 { get; set; }
        public string? TarjetaBin { get; set; }
        public bool Exitoso { get; set; }
        public int HttpStatusCode { get; set; }
        public string? ErrorTipo { get; set; }
        public decimal? Monto { get; set; }
        public string Moneda { get; set; } = "MXN";
        public int? DuracionMs { get; set; }
        public string? MetadataJson { get; set; }
    }

    #endregion
}
