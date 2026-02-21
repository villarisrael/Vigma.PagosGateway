using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vigma.PagosGateway.Controllers;
using Vigma.PagosGateway.Infrastructure;
using Vigma.PagosGateway.Services.Logging;

namespace Vigma.PagosGateway.Controllers.Stats
{
    /// <summary>
    /// Controlador para consultar estadísticas y logs de pagos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StatsController : AuthenticatedControllerBase
    {
        private readonly IPagoLoggerService _pagoLogger;
        private readonly TimbradoDbContext _db;

        public StatsController(
            IPagoLoggerService pagoLogger,
            TimbradoDbContext db,
            ILogger<StatsController> logger)
            : base(logger)
        {
            _pagoLogger = pagoLogger;
            _db = db;
        }

        /// <summary>
        /// GET: api/stats
        /// Obtiene estadísticas generales del tenant
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStats([FromQuery] int dias = 30)
        {
            var tenantId = GetTenantId();
            var stats = await _pagoLogger.GetStatsAsync(tenantId, dias);

            return Ok(new
            {
                ok = true,
                tenantId = tenantId,
                tenantNombre = GetTenantNombre(),
                stats = stats,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// GET: api/stats/transacciones
        /// Obtiene las últimas transacciones exitosas
        /// </summary>
        [HttpGet("transacciones")]
        public async Task<IActionResult> GetTransacciones(
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] int limit = 50)
        {
            var tenantId = GetTenantId();
            desde ??= DateTime.UtcNow.AddDays(-7);
            hasta ??= DateTime.UtcNow;

            var transacciones = await _db.PagosOkLog
                .Where(p => 
                    p.TenantId == tenantId &&
                    p.CreadoUtc >= desde &&
                    p.CreadoUtc <= hasta)
                .OrderByDescending(p => p.CreadoUtc)
                .Take(limit)
                .Select(p => new
                {
                    p.Id,
                    p.OrderId,
                    p.TransactionId,
                    p.Gateway,
                    p.TipoOperacion,
                    p.Monto,
                    p.Moneda,
                    p.TarjetaTipo,
                    p.TarjetaLast4,
                    p.GatewayCode,
                    p.ResultCode,
                    p.CreadoUtc,
                    p.DuracionMs
                })
                .ToListAsync();

            return Ok(new
            {
                ok = true,
                tenantId = tenantId,
                desde = desde,
                hasta = hasta,
                total = transacciones.Count,
                transacciones = transacciones
            });
        }

        /// <summary>
        /// GET: api/stats/errores
        /// Obtiene los últimos errores
        /// </summary>
        [HttpGet("errores")]
        public async Task<IActionResult> GetErrores(
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] int limit = 50)
        {
            var tenantId = GetTenantId();
            desde ??= DateTime.UtcNow.AddDays(-7);
            hasta ??= DateTime.UtcNow;

            var errores = await _db.PagosErrorLog
                .Where(p => 
                    p.TenantId == tenantId &&
                    p.CreadoUtc >= desde &&
                    p.CreadoUtc <= hasta)
                .OrderByDescending(p => p.CreadoUtc)
                .Take(limit)
                .Select(p => new
                {
                    p.Id,
                    p.OrderId,
                    p.TransactionId,
                    p.Gateway,
                    p.TipoOperacion,
                    p.Monto,
                    p.ErrorTipo,
                    p.ErrorCodigo,
                    p.ErrorMensaje,
                    p.GatewayCode,
                    p.HttpStatusCode,
                    p.CreadoUtc
                })
                .ToListAsync();

            return Ok(new
            {
                ok = true,
                tenantId = tenantId,
                desde = desde,
                hasta = hasta,
                total = errores.Count,
                errores = errores
            });
        }

        /// <summary>
        /// GET: api/stats/intentos
        /// Obtiene los últimos intentos (para análisis de fraude)
        /// </summary>
        [HttpGet("intentos")]
        public async Task<IActionResult> GetIntentos(
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] int limit = 100)
        {
            var tenantId = GetTenantId();
            desde ??= DateTime.UtcNow.AddHours(-24);
            hasta ??= DateTime.UtcNow;

            var intentos = await _db.PagosIntentoLog
                .Where(p => 
                    p.TenantId == tenantId &&
                    p.CreadoUtc >= desde &&
                    p.CreadoUtc <= hasta)
                .OrderByDescending(p => p.CreadoUtc)
                .Take(limit)
                .Select(p => new
                {
                    p.Id,
                    p.Gateway,
                    p.Endpoint,
                    p.TipoOperacion,
                    p.IpCliente,
                    p.TarjetaLast4,
                    p.Exitoso,
                    p.HttpStatusCode,
                    p.ErrorTipo,
                    p.CreadoUtc
                })
                .ToListAsync();

            return Ok(new
            {
                ok = true,
                tenantId = tenantId,
                desde = desde,
                hasta = hasta,
                total = intentos.Count,
                intentos = intentos
            });
        }

        /// <summary>
        /// GET: api/stats/dashboard
        /// Dashboard con métricas clave
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var tenantId = GetTenantId();
            var ahora = DateTime.UtcNow;

            // Últimas 24 horas
            var hace24h = ahora.AddHours(-24);
            var exitosos24h = await _db.PagosOkLog.CountAsync(p => 
                p.TenantId == tenantId && p.CreadoUtc >= hace24h);
            var errores24h = await _db.PagosErrorLog.CountAsync(p => 
                p.TenantId == tenantId && p.CreadoUtc >= hace24h);

            // Última semana
            var hace7d = ahora.AddDays(-7);
            var exitosos7d = await _db.PagosOkLog.CountAsync(p => 
                p.TenantId == tenantId && p.CreadoUtc >= hace7d);
            var errores7d = await _db.PagosErrorLog.CountAsync(p => 
                p.TenantId == tenantId && p.CreadoUtc >= hace7d);

            // Montos
            var montoHoy = await _db.PagosOkLog
                .Where(p => p.TenantId == tenantId && p.CreadoUtc >= hace24h)
                .SumAsync(p => (decimal?)p.Monto) ?? 0;

            var montoSemana = await _db.PagosOkLog
                .Where(p => p.TenantId == tenantId && p.CreadoUtc >= hace7d)
                .SumAsync(p => (decimal?)p.Monto) ?? 0;

            // Por gateway
            var porGateway = await _db.PagosOkLog
                .Where(p => p.TenantId == tenantId && p.CreadoUtc >= hace7d)
                .GroupBy(p => p.Gateway)
                .Select(g => new
                {
                    gateway = g.Key,
                    transacciones = g.Count(),
                    monto = g.Sum(p => p.Monto)
                })
                .ToListAsync();

            // Top errores
            var topErrores = await _db.PagosErrorLog
                .Where(p => p.TenantId == tenantId && p.CreadoUtc >= hace7d)
                .GroupBy(p => new { p.ErrorTipo, p.ErrorCodigo })
                .Select(g => new
                {
                    tipo = g.Key.ErrorTipo,
                    codigo = g.Key.ErrorCodigo,
                    cantidad = g.Count()
                })
                .OrderByDescending(x => x.cantidad)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                ok = true,
                tenantId = tenantId,
                tenantNombre = GetTenantNombre(),
                dashboard = new
                {
                    ultimas_24h = new
                    {
                        exitosos = exitosos24h,
                        errores = errores24h,
                        total = exitosos24h + errores24h,
                        tasa_exito = exitosos24h + errores24h > 0 
                            ? Math.Round((decimal)exitosos24h / (exitosos24h + errores24h) * 100, 2) 
                            : 0,
                        monto_total = montoHoy
                    },
                    ultima_semana = new
                    {
                        exitosos = exitosos7d,
                        errores = errores7d,
                        total = exitosos7d + errores7d,
                        tasa_exito = exitosos7d + errores7d > 0 
                            ? Math.Round((decimal)exitosos7d / (exitosos7d + errores7d) * 100, 2) 
                            : 0,
                        monto_total = montoSemana
                    },
                    por_gateway = porGateway,
                    top_errores = topErrores
                },
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// GET: api/stats/transaction/{orderId}/{transactionId}
        /// Obtiene detalles completos de una transacción
        /// </summary>
        [HttpGet("transaction/{orderId}/{transactionId}")]
        public async Task<IActionResult> GetTransactionDetails(string orderId, string transactionId)
        {
            var tenantId = GetTenantId();

            var transaction = await _db.PagosOkLog
                .FirstOrDefaultAsync(p => 
                    p.TenantId == tenantId &&
                    p.OrderId == orderId &&
                    p.TransactionId == transactionId);

            if (transaction == null)
            {
                return NotFound(new
                {
                    ok = false,
                    mensaje = "Transacción no encontrada"
                });
            }

            return Ok(new
            {
                ok = true,
                transaction = transaction
            });
        }
    }
}
