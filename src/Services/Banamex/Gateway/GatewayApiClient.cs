using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BanamexPaymentGateway.Services.Banamex.Gateway;

namespace gateway_csharp_sample_code.Gateway
{
    public class GatewayApiClient : AbstractGatewayApiClient
    {
        private readonly GatewayApiConfig GatewayApiConfig;

        public GatewayApiClient(IOptions<GatewayApiConfig> gatewayApiConfig, ILogger<GatewayApiClient> logger)
        {
            GatewayApiConfig = gatewayApiConfig.Value;
            this.Logger = logger;
        }

        /// <summary>
        /// Sends the transaction.
        /// </summary>
        /// <returns>The transaction.</returns>
        /// <param name="gatewayApiRequest">Gateway API request.</param>
        public override string SendTransaction(GatewayApiRequest gatewayApiRequest)
        {
            return this.executeHTTPMethod(gatewayApiRequest);
        }

    }
}
