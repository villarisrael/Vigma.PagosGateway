using System;
using Newtonsoft.Json.Linq;

namespace gateway_csharp_sample_code.Models
{
    public class InitiateBrowserPaymentResponse
    {
        public InitiateBrowserPaymentResponse()
        {
        }
        public String InteractionStatus { get; set; }
        public String Operation { get; set; }
        public String RedirectUrl { get; set; }
        public String OrderId { get; set; }
        public String ResponseCode { get; set; }
        public String Result { get; set; }
        public String TransactionId { get; set; }
        public String SourceType { get; set; }

        /// <summary>
        /// Convert Json to  initiate browser payment model response.
        /// </summary>
        /// <returns>The initiate browser payment response.</returns>
        /// <param name="response">Response.</param>
        public static InitiateBrowserPaymentResponse toInitiateBrowserPaymentResponse(string response) {

            InitiateBrowserPaymentResponse model = new InitiateBrowserPaymentResponse();
            JObject jObject = JObject.Parse(response);

            model.InteractionStatus = jObject["browserPayment"]["interaction"]["status"].Value<string>();
            model.Operation = jObject["browserPayment"]["operation"].Value<string>();
            model.RedirectUrl = jObject["browserPayment"]["redirectUrl"].Value<string>();
            model.OrderId = jObject["order"]["id"].Value<string>();
            model.ResponseCode = jObject["response"]["gatewayCode"].Value<string>();
            model.Result = jObject["result"].Value<string>();
            model.TransactionId = jObject["transaction"]["id"].Value<string>();
            model.SourceType = jObject["sourceOfFunds"]["type"].Value<string>();

            return model;

        }


    }
}
