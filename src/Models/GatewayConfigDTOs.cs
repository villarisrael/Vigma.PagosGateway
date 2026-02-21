namespace Vigma.PagosGateway.Models.GatewayConfigs
{
    /// <summary>
    /// Configuración de Banamex para un tenant
    /// </summary>
    public class BanamexTenantConfig
    {
        public string MerchantId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Version { get; set; } = "79";
        public string GatewayUrl { get; set; } = "https://banamex.dialectpayments.com";
        public string Currency { get; set; } = "MXN";
        public string? WebhookSecret { get; set; }
        
        // Opcionales
        public bool AuthenticationByCertificate { get; set; } = false;
        public string? CertificateLocation { get; set; }
        public string? CertificatePassword { get; set; }
        public bool UseProxy { get; set; } = false;
        public string? ProxyHost { get; set; }
        public int? ProxyPort { get; set; }
    }

    /// <summary>
    /// Configuración de Banorte para un tenant
    /// </summary>
    public class BanorteTenantConfig
    {
        public string MerchantId { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = "https://api.banorte.com/v1";
        public string? WebhookSecret { get; set; }
        
        // Campos específicos de Banorte
        public string? AfiliacionId { get; set; }
        public string? Sucursal { get; set; }
    }

    /// <summary>
    /// Configuración de Stripe para un tenant
    /// </summary>
    public class StripeTenantConfig
    {
        public string PublishableKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string? AccountId { get; set; } // Para Stripe Connect
        
        // Configuración adicional
        public string? StatementDescriptor { get; set; }
        public bool AutomaticTax { get; set; } = false;
    }
}
