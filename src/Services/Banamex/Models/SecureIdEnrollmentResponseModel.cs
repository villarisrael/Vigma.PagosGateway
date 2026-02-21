using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Microsoft.AspNetCore.Http;
using BanamexPaymentGateway.Utils;

namespace gateway_csharp_sample_code.Models
{
    public class SecureIdEnrollmentResponseModel
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string ResponseUrl { get; set; }
        public string AcsUrl { get; set; }
        public string PaReq { get; set; }
        public string MdValue { get; set; }

        public static SecureIdEnrollmentResponseModel toSecureIdEnrollmentResponseModel(HttpRequest Request, string response)
        {
            JObject jObject = JObject.Parse(response);

            SecureIdEnrollmentResponseModel model = new SecureIdEnrollmentResponseModel();
            model.Status = jObject["3DSecure"]["summaryStatus"].Value<string>();
            model.AcsUrl = jObject["3DSecure"]["authenticationRedirect"]["customized"]["acsUrl"].Value<string>();
            model.PaReq = jObject["3DSecure"]["authenticationRedirect"]["customized"]["paReq"].Value<string>();
            model.MdValue = IdUtils.GenerateSampleId();

            model.ResponseUrl = Microsoft.AspNetCore.Http.Extensions.UriHelper.BuildAbsolute(Request.Scheme, Request.Host, Request.PathBase, "/process3ds");

            return model;
        }
    }
}
