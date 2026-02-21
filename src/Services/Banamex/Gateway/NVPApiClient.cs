using System;
using System.Collections.Generic;
using System.Text;
using gateway_csharp_sample_code;
using gateway_csharp_sample_code.Gateway;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Web;
using BanamexPaymentGateway.Services.Banamex.Gateway;

namespace gateway_csharp_sample_code.Gateway
{
    public class NVPApiClient : AbstractGatewayApiClient
    {

        private readonly GatewayApiConfig GatewayApiConfig;
        public String CONTENT_TYPE { get; } = "application/x-www-form-urlencoded; charset=UTF-8"; 

        public NVPApiClient(){
            
        }

        public NVPApiClient(IOptions<GatewayApiConfig> gatewayApiConfig, ILogger<NVPApiClient> logger)
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
            //build NVP url
            gatewayApiRequest.buildRequestNPVUrl();

            //build NVP map
            gatewayApiRequest.buildNVPMap();


            ////set credentials
            gatewayApiRequest.NVPParameters.Add("apiUsername",GatewayApiConfig.Username );
            gatewayApiRequest.NVPParameters.Add("apiPassword", GatewayApiConfig.Password != null ? GatewayApiConfig.Password :  "" );

            //build payload
            gatewayApiRequest.Payload = buildNVPBody(gatewayApiRequest.NVPParameters);

            return executeHTTPMethod(gatewayApiRequest);

        }




        /// <summary>
        /// Builds the NVP Body.
        /// </summary>
        /// <returns>The NVP Body.</returns>
        /// <param name="nvpParameters">Nvp parameters.</param>
        private String buildNVPBody(Dictionary<String, String> nvpParameters){
                
             StringBuilder sb = new StringBuilder();

            //create NVP body
            foreach(var param in nvpParameters){
                
                if(sb.Length > 0){
                    sb.Append("&");
                }

                sb.AppendFormat( "{0}={1}", param.Key, HttpUtility.UrlEncode(param.Value)   );
            }

            return sb.ToString();
        } 

    }
}
