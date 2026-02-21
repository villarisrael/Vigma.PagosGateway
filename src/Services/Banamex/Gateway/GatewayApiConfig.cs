namespace BanamexPaymentGateway.Services.Banamex.Gateway
{
    public class GatewayApiConfig
    {
        public string Version { get; set; } = "79";
        public string MerchantId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string GatewayUrl { get; set; } = "https://banamex.dialectpayments.com";
        public string GatewayUrlCertificate { get; set; } = "https://banamex.dialectpayments.com";
        public string Currency { get; set; } = "MXN";
        
        // Autenticación por certificado
        public bool AuthenticationByCertificate { get; set; } = false;
        public string CertificateLocation { get; set; } = string.Empty;
        public string CertificatePassword { get; set; } = string.Empty;
        
        // Configuración de Proxy
        public bool UseProxy { get; set; } = false;
        public string ProxyHost { get; set; } = string.Empty;
        public int ProxyPort { get; set; } = 8080;
        public string ProxyUser { get; set; } = string.Empty;
        public string ProxyPassword { get; set; } = string.Empty;
        public string ProxyDomain { get; set; } = string.Empty;
        
        // SSL
        public bool IgnoreSslErrors { get; set; } = false;
        
        // Webhook
        public string WebhookSecret { get; set; } = string.Empty;
    }
}
