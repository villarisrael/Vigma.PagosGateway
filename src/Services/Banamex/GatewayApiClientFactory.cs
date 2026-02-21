// Archivo sugerido: Services/Banamex/GatewayApiClientFactory.cs
using BanamexPaymentGateway.Services.Banamex.Gateway;
using gateway_csharp_sample_code.Gateway;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vigma.PagosGateway.Services.Banamex
{
    public interface IGatewayApiClientFactory
    {
        GatewayApiClient Create(GatewayApiConfig cfg);
    }

    public class GatewayApiClientFactory : IGatewayApiClientFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public GatewayApiClientFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public GatewayApiClient Create(GatewayApiConfig cfg)
        {
            return new GatewayApiClient(
                Options.Create(cfg),
                _loggerFactory.CreateLogger<GatewayApiClient>());
        }
    }
}