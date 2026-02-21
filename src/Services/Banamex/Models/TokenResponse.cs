using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gateway_csharp_sample_code.Models
{
    public class TokenResponse
    {
        public string GatewayCode { get; set;  }
        public string Result { get; set; }
        public string Status { get; set; }
        public string Token { get; set; }

        /// <summary>
        /// Convert response JSON to TokenResponse object
        /// </summary>
        /// <returns>The Token response.</returns>
        /// <param name="response">Response.</param>
        public static TokenResponse ToTokenResponse(string response)
        {
            var model = new TokenResponse();

            JObject jObject = JObject.Parse(response);
            model.GatewayCode = jObject["response"]["gatewayCode"] != null ? jObject["response"]["gatewayCode"].ToString() : null;
            model.Result = jObject["result"] != null ? jObject["result"].ToString() : null;
            model.Status = jObject["status"] != null ? jObject["status"].ToString() : null;
            model.Token = jObject["token"] != null ? jObject["token"].ToString() : null;
            return model;
        }
    }
}
