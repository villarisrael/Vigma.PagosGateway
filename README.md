# Vigma.PagosGateway

**Plataforma Multi-Gateway de Procesamiento de Pagos**

Sistema unificado para procesamiento de pagos con múltiples gateways, autenticación centralizada por API Key y logging completo de transacciones.

---

## 🚀 Características

### ✅ Implementado

- **🔐 Autenticación Centralizada**
  - API Key con hash SHA256
  - Caché de 5 minutos para performance
  - Validación contra base de datos compartida con Vigma.TimbradoGateway

- **📊 Logging Completo**
  - Transacciones exitosas (`pagos_ok_log`)
  - Errores y declinaciones (`pagos_error_log`)
  - Todos los intentos (`pagos_intento_log`)
  - Webhooks recibidos (`pagos_webhook_log`)

- **💳 Gateway Banamex**
  - Pagos directos con tarjeta
  - Sesiones de checkout hospedado
  - Soporte para 3D Secure
  - Operaciones: PAY, AUTHORIZE, CAPTURE, REFUND, VOID

- **📈 Estadísticas y Analytics**
  - Dashboard en tiempo real
  - Reportes por tenant, gateway, período
  - Detección de fraude
  - Análisis de performance

### 🔜 Próximamente

- Gateway Banorte
- Gateway Stripe
- Rate limiting por tenant
- Webhooks salientes
- Panel de administración web

---

## 📋 Requisitos

- **.NET 8.0 SDK** o superior
- **MySQL 8.0** (base de datos compartida con Vigma.TimbradoGateway)
- **Visual Studio 2022** / **VS Code** / **Rider**

---

## 🛠️ Instalación

### 1. Clonar o extraer el proyecto

```bash
cd vigma-pagos-gateway
```

### 2. Ejecutar script SQL

Ejecuta el script para crear las tablas de logging:

```bash
mysql -h 217.15.168.43 -u tu_usuario -p timbrado_gateway < scripts/script_pagos_logging.sql
```

Esto creará:
- 4 tablas de logging
- 4 vistas para reporting
- 2 procedimientos almacenados
- Índices optimizados

### 3. Configurar credenciales

**Opción A: User Secrets (Recomendado para desarrollo)**

```bash
dotnet user-secrets init

# Base de datos
dotnet user-secrets set "ConnectionStrings:MySql" "Server=217.15.168.43;Database=timbrado_gateway;User=TU_USUARIO;Password=TU_PASSWORD;Port=3306;SslMode=None;"

# Banamex
dotnet user-secrets set "Banamex:GatewayApiConfig:MerchantId" "TU_MERCHANT_ID"
dotnet user-secrets set "Banamex:GatewayApiConfig:Username" "merchant.TU_MERCHANT_ID"
dotnet user-secrets set "Banamex:GatewayApiConfig:Password" "TU_PASSWORD"
```

**Opción B: Variables de ambiente**

```bash
# Windows PowerShell
$env:GATEWAY_BANAMEX__GATEWAYAPICONFIG__MERCHANTID = "TU_MERCHANT_ID"

# Linux/Mac
export GATEWAY_BANAMEX__GATEWAYAPICONFIG__MERCHANTID="TU_MERCHANT_ID"
```

### 4. Restaurar paquetes

```bash
dotnet restore
```

### 5. Compilar

```bash
dotnet build
```

### 6. Ejecutar

```bash
dotnet run
```

La aplicación estará disponible en:
- **HTTPS:** https://localhost:5001
- **HTTP:** http://localhost:5000
- **Swagger:** https://localhost:5001

---

## 🔑 Autenticación

Todas las peticiones a la API requieren un API Key válido.

### Headers aceptados:

```bash
# Opción 1: X-Api-Key
curl -H "X-Api-Key: LIVE_abc123..." https://localhost:5001/api/banamex/process

# Opción 2: X-API-KEY
curl -H "X-API-KEY: LIVE_abc123..." https://localhost:5001/api/banamex/process

# Opción 3: Authorization Bearer
curl -H "Authorization: Bearer LIVE_abc123..." https://localhost:5001/api/banamex/process
```

### Endpoints públicos (sin autenticación):

- `/health`
- `/swagger`
- `/swagger/v1/swagger.json`

---

## 💳 Uso de la API

### Procesar pago con Banamex

```bash
curl -X POST https://localhost:5001/api/banamex/process \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: TU_API_KEY" \
  -d '{
    "amount": "100.00",
    "cardNumber": "5123456789012346",
    "expiryMonth": "12",
    "expiryYear": "25",
    "securityCode": "123",
    "description": "Compra de prueba"
  }'
```

**Respuesta exitosa:**

```json
{
  "success": true,
  "orderId": "ORD-1708517400123",
  "transactionId": "TXN-1708517400456",
  "result": "SUCCESS",
  "gatewayCode": "APPROVED",
  "amount": "100.00",
  "currency": "MXN",
  "duracionMs": 1250
}
```

### Crear sesión de checkout

```bash
curl -X POST https://localhost:5001/api/banamex/create-session \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: TU_API_KEY" \
  -d '{
    "amount": "250.00",
    "description": "Pago seguro",
    "returnUrl": "https://tudominio.com/payment-success"
  }'
```

### Obtener estadísticas

```bash
# Dashboard general
curl -H "X-Api-Key: TU_API_KEY" https://localhost:5001/api/stats/dashboard

# Últimas transacciones
curl -H "X-Api-Key: TU_API_KEY" https://localhost:5001/api/stats/transacciones?limit=50

# Últimos errores
curl -H "X-Api-Key: TU_API_KEY" https://localhost:5001/api/stats/errores?dias=7

# Intentos (análisis de fraude)
curl -H "X-Api-Key: TU_API_KEY" https://localhost:5001/api/stats/intentos
```

---

## 📊 Logging y Auditoría

### Datos registrados automáticamente:

**Para CADA transacción exitosa:**
- ✅ Detalles completos del pago
- ✅ Tarjeta enmascarada (last4 + BIN)
- ✅ Respuesta del gateway
- ✅ Performance (duración en ms)
- ✅ IP del cliente, User-Agent
- ✅ Request/Response JSON

**Para CADA error:**
- ❌ Tipo de error (DECLINED, NETWORK_ERROR, TIMEOUT, etc.)
- ❌ Código y mensaje
- ❌ Stack trace (si aplica)
- ❌ Datos para reintentos

**Para CADA intento (exitoso o no):**
- 🔍 Endpoint, método HTTP
- 🔍 IP, tarjeta (last4)
- 🔍 Resultado (exitoso/fallido)
- 🔍 Útil para detección de fraude

### Consultas SQL útiles:

```sql
-- Transacciones del día
SELECT * FROM v_pagos_hoy_por_gateway;

-- Tasa de error últimas 24h
SELECT * FROM v_tasa_error_24h;

-- Top errores de la semana
SELECT * FROM v_top_errores_semana;

-- Detectar posible fraude
CALL sp_detectar_fraude(60, 5); -- últimos 60 min, mín 5 intentos
```

---

## 🏗️ Arquitectura del Proyecto

```
Vigma.PagosGateway/
├── src/
│   ├── Controllers/
│   │   ├── AuthenticatedControllerBase.cs      # Base para todos los controllers
│   │   ├── Banamex/
│   │   │   └── BanamexController.cs            # Endpoints de Banamex
│   │   └── Stats/
│   │       └── StatsController.cs              # Estadísticas y reportes
│   ├── Services/
│   │   ├── Authentication/
│   │   │   ├── IApiKeyValidator.cs
│   │   │   ├── ApiKeyValidator.cs              # Validación de API Keys
│   │   │   └── ApiKeyValidationResult.cs
│   │   ├── Logging/
│   │   │   └── PagoLoggerService.cs            # Servicio de logging
│   │   └── Banamex/
│   │       ├── Gateway/
│   │       │   ├── GatewayApiClient.cs
│   │       │   ├── NVPApiClient.cs
│   │       │   ├── GatewayApiConfig.cs
│   │       │   └── ...
│   │       └── Models/
│   │           └── ...
│   ├── Models/
│   │   ├── Tenant.cs                           # Modelo de tenant
│   │   └── Logging/
│   │       ├── PagoOkLog.cs
│   │       ├── PagoErrorLog.cs
│   │       └── PagoIntentoLog.cs
│   ├── Middleware/
│   │   ├── ApiKeyAuthenticationMiddleware.cs   # Autenticación
│   │   └── PagoLoggingMiddleware.cs            # Logging automático
│   ├── Infrastructure/
│   │   └── TimbradoDbContext.cs                # EF Core DbContext
│   ├── Utils/
│   │   ├── ApiKeyHelper.cs
│   │   └── IdUtils.cs
│   └── Program.cs                              # Configuración principal
├── scripts/
│   └── script_pagos_logging.sql                # Script de BD
├── docs/
│   └── ...
├── appsettings.json
├── Vigma.PagosGateway.csproj
└── README.md
```

---

## 🔒 Seguridad

### Mejores Prácticas Implementadas:

- ✅ **PCI-DSS Compliant:**
  - Nunca se guarda el número completo de tarjeta
  - Solo se almacena last4 + BIN (primeros 6 dígitos)
  - CVV NUNCA se guarda

- ✅ **Autenticación Segura:**
  - API Keys con hash SHA256
  - Caché temporal (5 minutos)
  - Validación en cada request

- ✅ **HTTPS Only:**
  - Forzar HTTPS en producción
  - Certificados SSL válidos

- ✅ **Logging Seguro:**
  - Request/Response sin datos sensibles
  - Stack traces solo en logs de error

- ✅ **Rate Limiting:**
  - Tabla `pagos_intento_log` para detectar abuso

### ⚠️ Antes de Producción:

1. Cambiar todos los valores `CONFIGURAR` en appsettings.json
2. Usar User Secrets o variables de ambiente para credenciales
3. Habilitar HTTPS estricto
4. Configurar rate limiting
5. Implementar alertas de fraude
6. Backup automático de la BD

---

## 📈 Estadísticas

### Dashboard API:

```json
{
  "dashboard": {
    "ultimas_24h": {
      "exitosos": 145,
      "errores": 8,
      "tasa_exito": 94.77,
      "monto_total": 45230.50
    },
    "por_gateway": [
      {
        "gateway": "banamex",
        "transacciones": 550,
        "monto": 180250.50
      }
    ],
    "top_errores": [
      {
        "tipo": "DECLINED",
        "codigo": "INSUFFICIENT_FUNDS",
        "cantidad": 28
      }
    ]
  }
}
```

---

## 🧪 Testing

### Tarjetas de Prueba:

```
VISA:             4111111111111111
Mastercard:       5123456789012346
American Express: 345678901234564

CVV: 123
Fecha: Cualquier fecha futura (MM/YY)
```

### Ejecutar tests:

```bash
# Verificar que la API funciona
curl https://localhost:5001/health

# Test con API Key (debe retornar info del tenant)
curl -H "X-Api-Key: TU_API_KEY" https://localhost:5001/api/stats/dashboard

# Test de pago (sandbox)
curl -X POST https://localhost:5001/api/banamex/process \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: TU_API_KEY" \
  -d '{
    "amount": "10.00",
    "cardNumber": "5123456789012346",
    "expiryMonth": "12",
    "expiryYear": "25",
    "securityCode": "123"
  }'
```

---

## 🛠️ Troubleshooting

### Error: "Connection to database failed"

- Verifica el connection string en appsettings.json o User Secrets
- Asegúrate de que MySQL esté accesible
- Revisa firewall y permisos

### Error: "API Key inválida"

- Verifica que el API Key sea correcto
- Comprueba que el tenant esté activo en la BD (`activo = 1`)
- Revisa los logs: `dotnet run --verbosity detailed`

### Los logs no se guardan:

- Verifica que las tablas existan: `SHOW TABLES LIKE 'pagos%';`
- Comprueba permisos de usuario MySQL
- Revisa logs de la aplicación para excepciones

### Swagger no carga:

- Solo disponible en Development
- Verifica que estés en: https://localhost:5001
- Comprueba que el proyecto compiló sin errores

---

## 📚 Documentación Adicional

- [INSTALACION.md](docs/INSTALACION.md) - Guía detallada de instalación
- [API_ENDPOINTS.md](docs/API_ENDPOINTS.md) - Documentación completa de endpoints
- [LOGGING.md](docs/LOGGING.md) - Sistema de logging y auditoría
- [SECURITY.md](docs/SECURITY.md) - Mejores prácticas de seguridad

---

## 🤝 Soporte

- **Email:** soporte@vigma.com
- **Documentación Banamex:** https://banamex.dialectpayments.com/api/documentation

---

## 📝 Licencia

Propiedad de Vigma. Todos los derechos reservados.

---

## ✨ Changelog

### Version 1.0.0 (2024-02-21)

- ✅ Implementación inicial
- ✅ Gateway Banamex completo
- ✅ Autenticación por API Key
- ✅ Sistema de logging completo
- ✅ Estadísticas y dashboard
- ✅ Detección de fraude
- ✅ Swagger integrado

---

Hecho con ❤️ por el equipo de Vigma
