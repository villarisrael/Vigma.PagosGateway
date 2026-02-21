using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vigma.PagosGateway.Models.Logging
{
    /// <summary>
    /// Registro de transacciones fallidas o con error
    /// </summary>
    [Table("pagos_error_log")]
    public class PagoErrorLog
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

        // IDs (pueden ser NULL si falló antes)
        [Column("order_id")]
        [MaxLength(100)]
        public string? OrderId { get; set; }

        [Column("transaction_id")]
        [MaxLength(100)]
        public string? TransactionId { get; set; }

        // Detalles del intento
        [Required]
        [Column("tipo_operacion")]
        [MaxLength(30)]
        public string TipoOperacion { get; set; } = string.Empty;

      
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Monto { get; set; }

        [Column("moneda")]
        [MaxLength(3)]
        public string Moneda { get; set; } = "MXN";

        // Información del error
        [Required]
        [Column("error_tipo")]
        [MaxLength(50)]
        public string ErrorTipo { get; set; } = string.Empty; // DECLINED, NETWORK_ERROR, VALIDATION, etc

        [Column("error_codigo")]
        [MaxLength(50)]
        public string? ErrorCodigo { get; set; }

        [Column("error_mensaje")]
        [MaxLength(1000)]
        public string? ErrorMensaje { get; set; }

        [Column("error_detalle")]
        public string? ErrorDetalle { get; set; }

        // Información de tarjeta (enmascarada)
        [Column("tarjeta_tipo")]
        [MaxLength(20)]
        public string? TarjetaTipo { get; set; }

        [Column("tarjeta_last4")]
        [MaxLength(4)]
        public string? TarjetaLast4 { get; set; }

        [Column("tarjeta_bin")]
        [MaxLength(6)]
        public string? TarjetaBin { get; set; }

        // Respuesta del gateway
        [Column("gateway_code")]
        [MaxLength(50)]
        public string? GatewayCode { get; set; }

        [Column("gateway_message")]
        [MaxLength(500)]
        public string? GatewayMessage { get; set; }

        [Column("http_status_code")]
        public int? HttpStatusCode { get; set; }

        // Información adicional
        [Column("ip_cliente")]
        [MaxLength(45)]
        public string? IpCliente { get; set; }

        [Column("user_agent")]
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        // Performance
        [Column("duracion_ms")]
        public int? DuracionMs { get; set; }

        [Column("servidor")]
        [MaxLength(80)]
        public string? Servidor { get; set; }

        // Datos raw
        [Column("request_json")]
        public string? RequestJson { get; set; }

        [Column("response_json")]
        public string? ResponseJson { get; set; }

        [Column("stack_trace")]
        public string? StackTrace { get; set; }

        [Column("metadata_json")]
        public string? MetadataJson { get; set; }

        // Reintentos
        [Column("intento_numero")]
        public int IntentoNumero { get; set; } = 1;

        [Column("reintentar")]
        public bool Reintentar { get; set; } = false;

        // Auditoría
        [Required]
        [Column("creado_utc")]
        public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
    }
}
