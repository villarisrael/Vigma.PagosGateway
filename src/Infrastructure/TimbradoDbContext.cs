using Microsoft.EntityFrameworkCore;
using Vigma.PagosGateway.Models;
using Vigma.PagosGateway.Models.Logging;

namespace Vigma.PagosGateway.Infrastructure
{
    public class TimbradoDbContext : DbContext
    {
        public TimbradoDbContext(DbContextOptions<TimbradoDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tenant> Tenants => Set<Tenant>();

        public DbSet<PagoOkLog> PagosOkLog => Set<PagoOkLog>();
        public DbSet<PagoErrorLog> PagosErrorLog => Set<PagoErrorLog>();
        public DbSet<PagoIntentoLog> PagosIntentoLog => Set<PagoIntentoLog>();

        public DbSet<TenantGatewayConfig> TenantGatewayConfigs => Set<TenantGatewayConfig>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Recomendación MySQL/Pomelo
            modelBuilder
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_0900_ai_ci");

            // -------------------------
            // Tenant
            // -------------------------
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.ToTable("tenants");
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.ApiKeyHash)
                    .IsUnique()
                    .HasDatabaseName("ux_tenants_api_key_hash");

                entity.HasIndex(e => e.Activo)
                    .HasDatabaseName("ix_tenants_activo");

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.ApiKeyHash)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsFixedLength();

                entity.Property(e => e.ApiKeyLast4)
                    .HasMaxLength(8);

                entity.Property(e => e.PacUsuario)
                    .HasMaxLength(80);

                entity.Property(e => e.Activo)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.PacProduccion)
                    .IsRequired()
                    .HasDefaultValue(false);

                // Si en BD es DATETIME(6) / TIMESTAMP(6)
                entity.Property(e => e.CreadoUtc)
                    .IsRequired()
                    .HasColumnType("datetime(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            });

            // -------------------------
            // pagos_ok_log
            // -------------------------
            modelBuilder.Entity<PagoOkLog>(entity =>
            {
                entity.ToTable("pagos_ok_log");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.TenantId, e.CreadoUtc })
                    .HasDatabaseName("ix_pagos_ok_tenant_creado");

                entity.HasIndex(e => e.OrderId)
                    .HasDatabaseName("ix_pagos_ok_order_id");

                entity.HasIndex(e => e.TransactionId)
                    .HasDatabaseName("ix_pagos_ok_transaction_id");

                entity.HasIndex(e => new { e.Gateway, e.Ambiente, e.CreadoUtc })
                    .HasDatabaseName("ix_pagos_ok_gateway_ambiente_creado");

                entity.Property(e => e.Monto).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RefundAmount).HasColumnType("decimal(18,2)");

                entity.Property(e => e.CreadoUtc)
                    .IsRequired()
                    .HasColumnType("datetime(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                // Si quieres que la BD lo actualice sola cuando hagas UPDATE:
                entity.Property(e => e.ActualizadoUtc)
                    .HasColumnType("datetime(6)")
                    .HasDefaultValueSql("NULL")
                    .ValueGeneratedOnAddOrUpdate();
                    

                // Opción alternativa MUY usada: "ON UPDATE CURRENT_TIMESTAMP(6)"
                // Nota: esto requiere que la columna exista así en tu migración/DDL.
                // entity.Property(e => e.ActualizadoUtc)
                //     .HasColumnType("datetime(6)")
                //     .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");
            });

            // -------------------------
            // pagos_error_log
            // -------------------------
            modelBuilder.Entity<PagoErrorLog>(entity =>
            {
                entity.ToTable("pagos_error_log");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.TenantId, e.CreadoUtc })
                    .HasDatabaseName("ix_pagos_error_tenant_creado");

                entity.HasIndex(e => new { e.Gateway, e.Ambiente, e.CreadoUtc })
                    .HasDatabaseName("ix_pagos_error_gateway_ambiente_creado");

                entity.HasIndex(e => e.OrderId)
                    .HasDatabaseName("ix_pagos_error_order_id");

                entity.HasIndex(e => e.TransactionId)
                    .HasDatabaseName("ix_pagos_error_transaction_id");

                entity.HasIndex(e => new { e.ErrorTipo, e.ErrorCodigo })
                    .HasDatabaseName("ix_pagos_error_tipo_codigo");

                entity.Property(e => e.Monto).HasColumnType("decimal(18,2)");

                entity.Property(e => e.CreadoUtc)
                    .IsRequired()
                    .HasColumnType("datetime(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            });

            // -------------------------
            // pagos_intento_log
            // -------------------------
            modelBuilder.Entity<PagoIntentoLog>(entity =>
            {
                entity.ToTable("pagos_intento_log");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.TenantId, e.CreadoUtc })
                    .HasDatabaseName("ix_pagos_intento_tenant_creado");

                entity.HasIndex(e => new { e.Gateway, e.Ambiente, e.CreadoUtc })
                    .HasDatabaseName("ix_pagos_intento_gateway_ambiente_creado");

                entity.HasIndex(e => e.OrderId)
                    .HasDatabaseName("ix_pagos_intento_order_id");

                entity.HasIndex(e => e.TransactionId)
                    .HasDatabaseName("ix_pagos_intento_transaction_id");

                entity.HasIndex(e => new { e.Exitoso, e.HttpStatusCode, e.CreadoUtc })
                    .HasDatabaseName("ix_pagos_intento_exitoso_http_creado");

                entity.Property(e => e.Monto).HasColumnType("decimal(18,2)");

                entity.Property(e => e.CreadoUtc)
                    .IsRequired()
                    .HasColumnType("datetime(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            });

            // -------------------------
            // tenant_gateway_config
            // -------------------------
            modelBuilder.Entity<TenantGatewayConfig>(entity =>
            {
                entity.ToTable("tenant_gateway_config");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Tenant)
                    .WithMany()
                    .HasForeignKey(e => e.TenantId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Evitar duplicado por tenant+gateway+ambiente (típico)
                entity.HasIndex(e => new { e.TenantId, e.Gateway, e.Ambiente })
                    .IsUnique()
                    .HasDatabaseName("ux_tenant_gateway_config_tenant_gateway_ambiente");

                // Índices útiles para filtros comunes
                entity.HasIndex(e => new { e.TenantId, e.Activo })
                    .HasDatabaseName("ix_tenant_gateway_config_tenant_activo");

                entity.HasIndex(e => new { e.Gateway, e.Ambiente, e.Activo })
                    .HasDatabaseName("ix_tenant_gateway_config_gateway_ambiente_activo");

                entity.Property(e => e.Gateway)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Ambiente)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.ConfigJsonEnc)
                    .IsRequired();

                entity.Property(e => e.MerchantId)
                    .HasMaxLength(100);

                entity.Property(e => e.WebhookSecretHash)
                    .HasMaxLength(64)
                    .IsFixedLength();

                entity.Property(e => e.MaxMontoTransaccion)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Activo)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.CreadoUtc)
                    .IsRequired()
                    .HasColumnType("datetime(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

                // Si quieres actualizado automático en BD (recomendado):
                // (requiere que tu migración cree la columna así)
                // entity.Property(e => e.ActualizadoUtc)
                //     .HasColumnType("datetime(6)")
                //     .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)");

                // Si lo vas a setear tú desde C# en cada update, deja esto simple:
                entity.Property(e => e.ActualizadoUtc)
                    .HasColumnType("datetime(6)");

                // opcional: index por merchant si lo usarás para búsquedas
                entity.HasIndex(e => e.MerchantId)
                    .HasDatabaseName("ix_tenant_gateway_config_merchant_id");
            });
        }
    }
}