using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gateway_csharp_sample_code.Models
{
    public class MasterpassWalletResponse
    {
        public string AllowedCardTypes { get; set;  }
        public string MerchantCheckoutId { get; set; }
        public string OriginUrl { get; set; }
        public string RequestToken { get; set; }

        /// <summary>
        /// Convert response JSON to MasterpassWalletResponse object
        /// </summary>
        /// <returns>The masterpass wallet response.</returns>
        /// <param name="response">Response.</param>
        public static MasterpassWalletResponse toMasterpassWalletResponse(string response)
        {
            JObject jObject = JObject.Parse(response);
            return jObject["wallet"]["masterpass"].ToObject<MasterpassWalletResponse>();
        }
    }
}
