using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vigma.PagosGateway.Models.Logging
{
    /// <summary>
    /// Registro de TODOS los intentos (exitosos o no)
    /// Útil para rate limiting, detección de fraude y analytics
    /// </summary>
    [Table("pagos_intento_log")]
    public class PagoIntentoLog
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        // Identificación
        [Required]
        [Column("tenant_id")]
        public int TenantId { get; set; }

        [Required]
        [Column("gateway")]
        [MaxLength(20)]
        public string Gateway { get; set; } = string.Empty;

        [Required]
        [Column("ambiente")]
        [MaxLength(10)]
        public string Ambiente { get; set; } = "test";

        // Request info
        [Required]
        [Column("endpoint")]
        [MaxLength(200)]
        public string Endpoint { get; set; } = string.Empty; // /api/banamex/pay

        [Required]
        [Column("metodo_http")]
        [MaxLength(10)]
        public string MetodoHttp { get; set; } = "POST";

        [Required]
        [Column("tipo_operacion")]
        [MaxLength(30)]
        public string TipoOperacion { get; set; } = string.Empty;

        // IDs
        [Column("order_id")]
        [MaxLength(100)]
        public string? OrderId { get; set; }

        [Column("transaction_id")]
        [MaxLength(100)]
        public string? TransactionId { get; set; }

        // Cliente
        [Required]
        [Column("ip_cliente")]
        [MaxLength(45)]
        public string IpCliente { get; set; } = string.Empty;

        [Column("user_agent")]
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        [Column("api_key_last4")]
        [MaxLength(8)]
        public string? ApiKeyLast4 { get; set; }

        // Tarjeta (enmascarada)
        [Column("tarjeta_last4")]
        [MaxLength(4)]
        public string? TarjetaLast4 { get; set; }

        [Column("tarjeta_bin")]
        [MaxLength(6)]
        public string? TarjetaBin { get; set; }

        // Resultado
        [Required]
        [Column("exitoso")]
        public bool Exitoso { get; set; } = false;

        [Required]
        [Column("http_status_code")]
        public int HttpStatusCode { get; set; }

        [Column("error_tipo")]
        [MaxLength(50)]
        public string? ErrorTipo { get; set; }

        // Monto
     
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Monto { get; set; }

        [Column("moneda")]
        [MaxLength(3)]
        public string Moneda { get; set; } = "MXN";

        // Performance
        [Column("duracion_ms")]
        public int? DuracionMs { get; set; }

        // Metadata mínima
        [Column("metadata_json")]
        public string? MetadataJson { get; set; }

        // Auditoría
        [Required]
        [Column("creado_utc")]
        public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
    }
}
