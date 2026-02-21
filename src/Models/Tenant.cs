using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vigma.PagosGateway.Models
{
    /// <summary>
    /// Modelo Tenant - Debe coincidir con la tabla 'tenants' de timbrado_gateway
    /// </summary>
    [Table("tenants")]
    public class Tenant
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("nombre")]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Column("api_key_hash")]
        [MaxLength(64)]
        public string ApiKeyHash { get; set; } = string.Empty;

        [Column("api_key_enc")]
        public string? ApiKeyEnc { get; set; }

        [Column("api_key_last4")]
        [MaxLength(8)]
        public string? ApiKeyLast4 { get; set; }

        [Column("api_key_rotated_utc")]
        public DateTime? ApiKeyRotatedUtc { get; set; }

        [Required]
        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Required]
        [Column("creado_utc")]
        public DateTime CreadoUtc { get; set; } = DateTime.UtcNow;

        [Column("actualizado_utc")]
        public DateTime? ActualizadoUtc { get; set; }

        [Column("pac_usuario")]
        [MaxLength(80)]
        public string? PacUsuario { get; set; }

        [Column("pac_password_enc")]
        public string? PacPasswordEnc { get; set; }

        [Required]
        [Column("pac_produccion")]
        public bool PacProduccion { get; set; } = false;
    }
}
