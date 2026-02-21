using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vigma.PagosGateway.Models.Logging
{
    /// <summary>
    /// Registro de transacciones exitosas
    /// </summary>
    [Table("pagos_ok_log")]
    public class PagoOkLog
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
        public string Gateway { get; set; } = string.Empty; // banamex, banorte, stripe

        [Required]
        [Column("ambiente")]
        [MaxLength(10)]
        public string Ambiente { get; set; } = "test"; // test, produccion

        // IDs de transacción
        [Required]
        [Column("order_id")]
        [MaxLength(100)]
        public string OrderId { get; set; } = string.Empty;

        [Required]
        [Column("transaction_id")]
        [MaxLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        [Column("gateway_transaction_id")]
        [MaxLength(150)]
        public string? GatewayTransactionId { get; set; }

        // Detalles del pago
        [Required]
        [Column("tipo_operacion")]
        [MaxLength(30)]
        public string TipoOperacion { get; set; } = string.Empty; // PAY, AUTHORIZE, CAPTURE, REFUND, VOID

        [Required]
      
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Required]
        [Column("moneda")]
        [MaxLength(3)]
        public string Moneda { get; set; } = "MXN";

        [Column("descripcion")]
        [MaxLength(500)]
        public string? Descripcion { get; set; }

        // Información de tarjeta (enmascarada)
        [Column("tarjeta_tipo")]
        [MaxLength(20)]
        public string? TarjetaTipo { get; set; } // VISA, MASTERCARD, AMEX

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

        [Column("result_code")]
        [MaxLength(20)]
        public string? ResultCode { get; set; }

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

        // Datos raw (JSON)
        [Column("request_json")]
        public string? RequestJson { get; set; }

        [Column("response_json")]
        public string? ResponseJson { get; set; }

        [Column("metadata_json")]
        public string? MetadataJson { get; set; }

        // Estado
        [Column("refunded")]
        public bool Refunded { get; set; } = false;


        [Column("refund_amount")]
        public decimal? RefundAmount { get; set; }

        [Column("refund_date")]
        public DateTime? RefundDate { get; set; }

        [Column("voided")]
        public bool Voided { get; set; } = false;

        [Column("void_date")]
        public DateTime? VoidDate { get; set; }

        // Auditoría
        [Required]
        [Column("creado_utc")]
        public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;

        [Column("actualizado_utc")]
        public DateTime? ActualizadoUtc { get; set; }

        // Navigation properties
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
    }
}
