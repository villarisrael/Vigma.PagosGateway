using BanamexPaymentGateway.Services.Banamex.Gateway;
using BanamexPaymentGateway.Utils;
using gateway_csharp_sample_code.Gateway;
using gateway_csharp_sample_code.Models;
using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.Extensions.Options;

using Vigma.PagosGateway.Services.Configuration;
using Vigma.PagosGateway.Services.Logging;
using Vigma.PagosGateway.Utils;

namespace Vigma.PagosGateway.Controllers.Banamex
{
    [ApiController]
    [Route("api/banamex")]
    public class BanamexController : AuthenticatedControllerBase
    {
        private readonly IGatewayConfigService _configService;
        private readonly IPagoLoggerService _pagoLogger;

        private readonly ILoggerFactory _loggerFactory;

        public BanamexController(
            IGatewayConfigService configService,
            IPagoLoggerService pagoLogger,
            ILogger<BanamexController> logger,
            ILoggerFactory loggerFactory)
            : base(logger)
        {
            _configService = configService;
            _pagoLogger = pagoLogger;
            _loggerFactory = loggerFactory;
        }

        /// <summary>
        /// POST: api/banamex/process
        /// Procesar pago directo con tarjeta
        /// </summary>
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var tenantId = GetTenantId();
            var isProd = IsPacProduccion();
            var ambiente = isProd ? "produccion" : "test";

            // 1. Verificar que el tenant tenga Banamex configurado
            var hasBanamex = await _configService.TenantHasGatewayAsync(tenantId, "banamex", ambiente);
            if (!hasBanamex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = "Gateway no configurado",
                    mensaje = $"Tu cuenta no tiene Banamex configurado en ambiente {ambiente}. Contacta a soporte."
                });
            }

            // 2. Obtener configuración específica del tenant
            var banamexConfig = await _configService.GetBanamexConfigAsync(tenantId, ambiente);
            if (banamexConfig == null)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "Error de configuración",
                    mensaje = "No se pudo obtener la configuración de Banamex"
                });
            }

            var orderId = request.OrderId ?? IdUtils.GenerateOrderId();
            var transactionId = IdUtils.GenerateTransactionId();

            LogWithTenant("Procesando pago Banamex - Order: {OrderId}, Monto: {Amount}", orderId, request.Amount);

            try
            {
                // 3. Crear cliente con la configuración del tenant
                var gatewayApiConfig = new GatewayApiConfig
                {
                    MerchantId = banamexConfig.MerchantId,
                    Username = banamexConfig.Username,
                    Password = banamexConfig.Password,
                    Version = banamexConfig.Version,
                    GatewayUrl = banamexConfig.GatewayUrl,
                    Currency = banamexConfig.Currency,
                    AuthenticationByCertificate = banamexConfig.AuthenticationByCertificate,
                    UseProxy = banamexConfig.UseProxy
                };

                var gatewayClient = new GatewayApiClient(
                         Options.Create(gatewayApiConfig),
                         _loggerFactory.CreateLogger<GatewayApiClient>()
                     );


                // 4. Preparar request
                var gatewayRequest = new GatewayApiRequest(gatewayApiConfig)
                {
                    ApiOperation = "PAY",
                    OrderId = orderId,
                    TransactionId = transactionId,
                    OrderAmount = request.Amount,
                    OrderCurrency = request.Currency ?? banamexConfig.Currency,
                    OrderDescription = request.Description ?? "Pago con tarjeta",
                    SourceType = "CARD",
                    CardNumber = request.CardNumber,
                    ExpiryMonth = request.ExpiryMonth,
                    ExpiryYear = request.ExpiryYear,
                    SecurityCode = request.SecurityCode,
                    ApiMethod = "PUT"
                };

                gatewayRequest.buildRequestUrl();
                gatewayRequest.buildPayload();

                // 5. Ejecutar transacción
                string response = gatewayClient.SendTransaction(gatewayRequest);
                stopwatch.Stop();

                // 6. Procesar respuesta
                if (JsonHelper.isErrorMessage(response))
                {
                    var error = ErrorViewModel.toErrorViewModel(Guid.NewGuid().ToString(), response);

                    await _pagoLogger.LogErrorAsync(new PagoErrorData
                    {
                        TenantId = tenantId,
                        Gateway = "banamex",
                        Ambiente = ambiente,
                        OrderId = orderId,
                        TransactionId = transactionId,
                        TipoOperacion = "PAY",
                        Monto = decimal.Parse(request.Amount),
                        Moneda = request.Currency ?? banamexConfig.Currency,
                        ErrorTipo = "GATEWAY_ERROR",
                        ErrorCodigo = error.Cause,
                        ErrorMensaje = error.Explanation ?? error.Message,
                        TarjetaLast4 = request.CardNumber.Substring(request.CardNumber.Length - 4),
                        TarjetaBin = request.CardNumber.Substring(0, 6),
                        HttpStatusCode = 400,
                        IpCliente = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        DuracionMs = (int)stopwatch.ElapsedMilliseconds,
                        ResponseJson = response
                    });

                    return BadRequest(new { success = false, error = error.Explanation, details = error });
                }

                var responseData = JsonConvert.DeserializeObject<dynamic>(response);
                var result = responseData?.result?.ToString();
                var gatewayCode = responseData?.response?.gatewayCode?.ToString();

                if (result == "SUCCESS")
                {
                    // ✅ Pago exitoso
                    await _pagoLogger.LogExitosoAsync(new PagoExitosoData
                    {
                        TenantId = tenantId,
                        Gateway = "banamex",
                        Ambiente = ambiente,
                        OrderId = orderId,
                        TransactionId = transactionId,
                        GatewayTransactionId = responseData?.transaction?.id?.ToString(),
                        TipoOperacion = "PAY",
                        Monto = decimal.Parse(request.Amount),
                        Moneda = request.Currency ?? banamexConfig.Currency,
                        Descripcion = request.Description,
                        TarjetaTipo = ExtractCardType(request.CardNumber),
                        TarjetaLast4 = request.CardNumber.Substring(request.CardNumber.Length - 4),
                        TarjetaBin = request.CardNumber.Substring(0, 6),
                        GatewayCode = gatewayCode,
                        GatewayMessage = responseData?.response?.gatewayMessage?.ToString(),
                        ResultCode = result,
                        IpCliente = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        DuracionMs = (int)stopwatch.ElapsedMilliseconds,
                        ResponseJson = response
                    });

                    LogWithTenant("✅ Pago Banamex exitoso - Order: {OrderId}, {Duration}ms", orderId, stopwatch.ElapsedMilliseconds);

                    return Ok(new
                    {
                        success = true,
                        orderId = orderId,
                        transactionId = transactionId,
                        result = result,
                        gatewayCode = gatewayCode,
                        amount = request.Amount,
                        currency = request.Currency ?? banamexConfig.Currency,
                        duracionMs = stopwatch.ElapsedMilliseconds
                    });
                }
                else
                {
                    // ❌ Pago declinado
                    await _pagoLogger.LogErrorAsync(new PagoErrorData
                    {
                        TenantId = tenantId,
                        Gateway = "banamex",
                        Ambiente = ambiente,
                        OrderId = orderId,
                        TransactionId = transactionId,
                        TipoOperacion = "PAY",
                        Monto = decimal.Parse(request.Amount),
                        ErrorTipo = "DECLINED",
                        ErrorCodigo = gatewayCode,
                        ErrorMensaje = responseData?.response?.gatewayMessage?.ToString(),
                        TarjetaLast4 = request.CardNumber.Substring(request.CardNumber.Length - 4),
                        GatewayCode = gatewayCode,
                        HttpStatusCode = 200,
                        IpCliente = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        DuracionMs = (int)stopwatch.ElapsedMilliseconds,
                        ResponseJson = response
                    });

                    return BadRequest(new { success = false, error = "Pago declinado", gatewayCode = gatewayCode });
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                await _pagoLogger.LogErrorAsync(new PagoErrorData
                {
                    TenantId = tenantId,
                    Gateway = "banamex",
                    Ambiente = ambiente,
                    OrderId = orderId,
                    TipoOperacion = "PAY",
                    Monto = decimal.Parse(request.Amount),
                    ErrorTipo = "GATEWAY_ERROR",
                    ErrorMensaje = ex.Message,
                    HttpStatusCode = 500,
                    DuracionMs = (int)stopwatch.ElapsedMilliseconds,
                    StackTrace = ex.StackTrace
                });

                LogErrorWithTenant(ex, "Error al procesar pago Banamex");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        private string ExtractCardType(string cardNumber)
        {
            if (cardNumber.StartsWith("4")) return "VISA";
            if (cardNumber.StartsWith("5")) return "MASTERCARD";
            if (cardNumber.StartsWith("34") || cardNumber.StartsWith("37")) return "AMEX";
            return "UNKNOWN";
        }
    }

    #region Request Models

    public class ProcessPaymentRequest
    {
        public string? OrderId { get; set; }
        public string Amount { get; set; } = "100.00";
        public string? Currency { get; set; }
        public string? Description { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string ExpiryMonth { get; set; } = string.Empty;
        public string ExpiryYear { get; set; } = string.Empty;
        public string SecurityCode { get; set; } = string.Empty;
    }

    #endregion
}
