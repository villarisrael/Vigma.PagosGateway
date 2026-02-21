using BanamexPaymentGateway.Services.Banamex.Gateway;
using gateway_csharp_sample_code.Gateway;
using Microsoft.EntityFrameworkCore;
using Vigma.PagosGateway.Infrastructure;
using Vigma.PagosGateway.Middleware;
using Vigma.PagosGateway.Services.Authentication;
// ✅ Factory
using Vigma.PagosGateway.Services.Banamex;
using Vigma.PagosGateway.Services.Configuration;
using Vigma.PagosGateway.Services.Logging;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURACIÓN =====
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(prefix: "GATEWAY_")
    .AddUserSecrets<Program>(optional: true);

// ===== BASE DE DATOS =====
var connectionString = builder.Configuration.GetConnectionString("MySql")
    ?? throw new InvalidOperationException("Connection string 'MySql' no encontrado");

builder.Services.AddDbContext<TimbradoDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ===== CACHE =====
builder.Services.AddMemoryCache();

// ===== SERVICIOS DE AUTENTICACIÓN =====
builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

// ===== SERVICIOS DE LOGGING =====
builder.Services.AddScoped<IPagoLoggerService, PagoLoggerService>();

// ===== CONFIGURACIÓN DE GATEWAYS =====
// Banamex: clientes utilitarios del SDK (si los ocupas)
builder.Services.AddScoped<NVPApiClient>();


// ✅ Banamex: Factory para crear GatewayApiClient por tenant
builder.Services.AddSingleton<IGatewayApiClientFactory, GatewayApiClientFactory>();


// ===== LOGGING =====
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
builder.Logging.AddFilter("Vigma.PagosGateway", LogLevel.Debug);

// ===== CONTROLLERS =====
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

// ===== SWAGGER =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Vigma.PagosGateway API",
        Version = "v1",
        Description = @"**API Unificada para Procesamiento de Pagos**

Plataforma multi-gateway con autenticación por API Key y logging completo.

**Autenticación:**
- Incluye el header `X-Api-Key: tu-api-key` en todas las peticiones
- También acepta: `Authorization: Bearer {api-key}`

**Gateways Soportados:**
- ✅ Banamex (Mastercard Payment Gateway Services)
- 🔜 Banorte (próximamente)
- 🔜 Stripe (próximamente)",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Vigma Soporte",
            Email = "soporte@vigma.com"
        }
    });

    options.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-Api-Key",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "API Key necesaria para autenticación. Ejemplo: `X-Api-Key: LIVE_abc123...`"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });

    options.CustomSchemaIds(type => type.FullName);
});

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ===== HEALTH CHECKS =====
// Requiere: Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TimbradoDbContext>("database");

var app = builder.Build();

// ===== VALIDAR CONFIGURACIÓN AL INICIO =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Verificar base de datos
        var db = services.GetRequiredService<TimbradoDbContext>();
        var canConnect = await db.Database.CanConnectAsync();

        if (canConnect)
        {
            logger.LogInformation("✅ Conexión a base de datos establecida");
            var tenantsCount = await db.Tenants.CountAsync();
            logger.LogInformation("   📊 Tenants registrados: {Count}", tenantsCount);
        }
        else
        {
            logger.LogWarning("⚠️  No se pudo conectar a la base de datos");
        }

        // Verificar servicios
        _ = services.GetRequiredService<IApiKeyValidator>();
        logger.LogInformation("✅ ApiKeyValidator registrado");

        _ = services.GetRequiredService<IPagoLoggerService>();
        logger.LogInformation("✅ PagoLoggerService registrado");

        // ✅ Verificar factory (en lugar de GatewayApiClient)
        _ = services.GetRequiredService<IGatewayApiClientFactory>();
        logger.LogInformation("✅ GatewayApiClientFactory registrada");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error al validar servicios en el inicio");
    }
}

// ===== MIDDLEWARE PIPELINE =====
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vigma.PagosGateway v1");
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "Vigma.PagosGateway API";
        options.DisplayRequestDuration();
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// ===== MIDDLEWARE DE AUTENTICACIÓN Y LOGGING (ORDEN IMPORTANTE) =====
app.UseApiKeyAuthentication();  // 1) autenticar
app.UsePagoLogging();           // 2) log de intentos

app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/health");

// ===== LOGGING DE INICIO =====
app.Logger.LogInformation("");
app.Logger.LogInformation("╔══════════════════════════════════════════════════════════════╗");
app.Logger.LogInformation("║         VIGMA.PAGOSGATEWAY - INICIADO CORRECTAMENTE          ║");
app.Logger.LogInformation("╚══════════════════════════════════════════════════════════════╝");
app.Logger.LogInformation("");
app.Logger.LogInformation("🌐 Ambiente: {Environment}", app.Environment.EnvironmentName);
app.Logger.LogInformation("🔗 URLs: {Urls}", string.Join(", ", app.Urls));
app.Logger.LogInformation("📚 Swagger: {Swagger}", app.Environment.IsDevelopment() ? "Habilitado" : "Deshabilitado");
app.Logger.LogInformation("🔐 Autenticación: API Key (X-Api-Key header)");
app.Logger.LogInformation("📊 Logging: Habilitado (pagos_ok_log, pagos_error_log, pagos_intento_log)");
app.Logger.LogInformation("💳 Gateways: Banamex ✅ | Banorte 🔜 | Stripe 🔜");
app.Logger.LogInformation("");

app.Run();