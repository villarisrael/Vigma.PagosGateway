using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vigma.PagosGateway.Models
{
    /// <summary>
    /// Configuración de gateway específica para cada tenant
    /// </summary>
    [Table("tenant_gateway_config")]
    public class TenantGatewayConfig
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("tenant_id")]
        public int TenantId { get; set; }

        [Required]
        [Column("gateway")]
        [MaxLength(20)]
        public string Gateway { get; set; } = string.Empty; // banamex, banorte, stripe

        [Required]
        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Required]
        [Column("ambiente")]
        [MaxLength(10)]
        public string Ambiente { get; set; } = "test"; // test, produccion

        [Required]
        [Column("config_json_enc")]
        public string ConfigJsonEnc { get; set; } = string.Empty; // JSON cifrado

        [Column("merchant_id")]
        [MaxLength(100)]
        public string? MerchantId { get; set; }

        [Column("webhook_secret_hash")]
        [MaxLength(64)]
        public string? WebhookSecretHash { get; set; }

      
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxMontoTransaccion { get; set; }

        [Column("max_transacciones_dia")]
        public int? MaxTransaccionesDia { get; set; }

        [Required]
        [Column("creado_utc")]
        public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;

        [Column("actualizado_utc")]
        public DateTime? ActualizadoUtc { get; set; }

        [Column("creado_por_usuario_id")]
        public long? CreadoPorUsuarioId { get; set; }

        // Navigation property
        [ForeignKey("TenantId")]
        public virtual Tenant? Tenant { get; set; }
    }
}
