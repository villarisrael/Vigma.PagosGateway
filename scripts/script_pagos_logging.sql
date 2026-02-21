-- ============================================================
-- VIGMA.PAGOSGATEWAY - TABLAS DE LOGGING Y AUDITORÍA
-- Agregar a la base de datos: timbrado_gateway
-- ============================================================

-- ============================================================
-- Tabla: pagos_ok_log
-- Registro de transacciones exitosas de todos los gateways
-- ============================================================
DROP TABLE IF EXISTS `pagos_ok_log`;
CREATE TABLE `pagos_ok_log` (
  `id` BIGINT NOT NULL AUTO_INCREMENT,
  
  -- Identificación
  `tenant_id` INT NOT NULL,
  `gateway` VARCHAR(20) NOT NULL COMMENT 'banamex, banorte, stripe',
  `ambiente` VARCHAR(10) NOT NULL DEFAULT 'test' COMMENT 'test, produccion',
  
  -- IDs de la transacción
  `order_id` VARCHAR(100) NOT NULL,
  `transaction_id` VARCHAR(100) NOT NULL,
  `gateway_transaction_id` VARCHAR(150) DEFAULT NULL COMMENT 'ID del gateway externo',
  
  -- Detalles del pago
  `tipo_operacion` VARCHAR(30) NOT NULL COMMENT 'PAY, AUTHORIZE, CAPTURE, REFUND, VOID',
  `monto` DECIMAL(18,2) NOT NULL,
  `moneda` VARCHAR(3) NOT NULL DEFAULT 'MXN',
  `descripcion` VARCHAR(500) DEFAULT NULL,
  
  -- Información de tarjeta (enmascarada)
  `tarjeta_tipo` VARCHAR(20) DEFAULT NULL COMMENT 'VISA, MASTERCARD, AMEX',
  `tarjeta_last4` VARCHAR(4) DEFAULT NULL,
  `tarjeta_bin` VARCHAR(6) DEFAULT NULL COMMENT 'Primeros 6 dígitos',
  
  -- Respuesta del gateway
  `gateway_code` VARCHAR(50) DEFAULT NULL,
  `gateway_message` VARCHAR(500) DEFAULT NULL,
  `result_code` VARCHAR(20) DEFAULT NULL COMMENT 'SUCCESS, APPROVED, etc',
  
  -- Información adicional
  `ip_cliente` VARCHAR(45) DEFAULT NULL,
  `user_agent` VARCHAR(500) DEFAULT NULL,
  
  -- Performance
  `duracion_ms` INT DEFAULT NULL COMMENT 'Tiempo de respuesta en milisegundos',
  `servidor` VARCHAR(80) DEFAULT NULL,
  
  -- Datos raw (JSON)
  `request_json` LONGTEXT COMMENT 'Request enviado al gateway',
  `response_json` LONGTEXT COMMENT 'Respuesta completa del gateway',
  
  -- Metadata adicional
  `metadata_json` LONGTEXT COMMENT 'Headers adicionales, datos custom',
  
  -- Estado
  `refunded` TINYINT(1) NOT NULL DEFAULT 0,
  `refund_amount` DECIMAL(18,2) DEFAULT NULL,
  `refund_date` DATETIME DEFAULT NULL,
  
  `voided` TINYINT(1) NOT NULL DEFAULT 0,
  `void_date` DATETIME DEFAULT NULL,
  
  -- Auditoría
  `creado_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `actualizado_utc` DATETIME(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  
  PRIMARY KEY (`id`),
  
  -- Índices para búsquedas comunes
  INDEX `ix_pagosok_tenant_created` (`tenant_id`, `creado_utc`),
  INDEX `ix_pagosok_gateway_created` (`gateway`, `creado_utc`),
  INDEX `ix_pagosok_order` (`order_id`),
  INDEX `ix_pagosok_transaction` (`transaction_id`),
  INDEX `ix_pagosok_gateway_txn` (`gateway_transaction_id`),
  INDEX `ix_pagosok_created` (`creado_utc`),
  INDEX `ix_pagosok_monto` (`monto`),
  INDEX `ix_pagosok_tipo` (`tipo_operacion`),
  INDEX `ix_pagosok_refunded` (`refunded`),
  INDEX `ix_pagosok_tarjeta` (`tarjeta_last4`),
  
  -- FK a tenants
  CONSTRAINT `fk_pagosok_tenant` FOREIGN KEY (`tenant_id`) 
    REFERENCES `tenants` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
    
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='Registro de transacciones exitosas de todos los gateways';


-- ============================================================
-- Tabla: pagos_error_log
-- Registro de transacciones fallidas o con error
-- ============================================================
DROP TABLE IF EXISTS `pagos_error_log`;
CREATE TABLE `pagos_error_log` (
  `id` BIGINT NOT NULL AUTO_INCREMENT,
  
  -- Identificación
  `tenant_id` INT NOT NULL,
  `gateway` VARCHAR(20) NOT NULL COMMENT 'banamex, banorte, stripe',
  `ambiente` VARCHAR(10) NOT NULL DEFAULT 'test',
  
  -- IDs de la transacción (pueden ser NULL si falló antes)
  `order_id` VARCHAR(100) DEFAULT NULL,
  `transaction_id` VARCHAR(100) DEFAULT NULL,
  
  -- Detalles del intento
  `tipo_operacion` VARCHAR(30) NOT NULL,
  `monto` DECIMAL(18,2) DEFAULT NULL,
  `moneda` VARCHAR(3) DEFAULT 'MXN',
  
  -- Información del error
  `error_tipo` VARCHAR(50) NOT NULL COMMENT 'DECLINED, NETWORK_ERROR, VALIDATION, GATEWAY_ERROR, TIMEOUT',
  `error_codigo` VARCHAR(50) DEFAULT NULL,
  `error_mensaje` VARCHAR(1000) DEFAULT NULL,
  `error_detalle` TEXT DEFAULT NULL,
  
  -- Información de tarjeta (enmascarada)
  `tarjeta_tipo` VARCHAR(20) DEFAULT NULL,
  `tarjeta_last4` VARCHAR(4) DEFAULT NULL,
  `tarjeta_bin` VARCHAR(6) DEFAULT NULL,
  
  -- Respuesta del gateway
  `gateway_code` VARCHAR(50) DEFAULT NULL,
  `gateway_message` VARCHAR(500) DEFAULT NULL,
  `http_status_code` INT DEFAULT NULL,
  
  -- Información adicional
  `ip_cliente` VARCHAR(45) DEFAULT NULL,
  `user_agent` VARCHAR(500) DEFAULT NULL,
  
  -- Performance
  `duracion_ms` INT DEFAULT NULL,
  `servidor` VARCHAR(80) DEFAULT NULL,
  
  -- Datos raw (JSON)
  `request_json` LONGTEXT COMMENT 'Request que causó el error',
  `response_json` LONGTEXT COMMENT 'Respuesta de error del gateway',
  `stack_trace` TEXT COMMENT 'Stack trace si aplica',
  
  -- Metadata
  `metadata_json` LONGTEXT,
  
  -- Intentos y retry
  `intento_numero` INT DEFAULT 1 COMMENT 'Número de intento (para retries)',
  `reintentar` TINYINT(1) DEFAULT 0 COMMENT 'Si se debe reintentar',
  
  -- Auditoría
  `creado_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  
  PRIMARY KEY (`id`),
  
  -- Índices
  INDEX `ix_pagoserr_tenant_created` (`tenant_id`, `creado_utc`),
  INDEX `ix_pagoserr_gateway_created` (`gateway`, `creado_utc`),
  INDEX `ix_pagoserr_tipo_error` (`error_tipo`, `creado_utc`),
  INDEX `ix_pagoserr_order` (`order_id`),
  INDEX `ix_pagoserr_transaction` (`transaction_id`),
  INDEX `ix_pagoserr_created` (`creado_utc`),
  INDEX `ix_pagoserr_codigo` (`error_codigo`),
  INDEX `ix_pagoserr_tarjeta` (`tarjeta_last4`),
  INDEX `ix_pagoserr_reintentar` (`reintentar`),
  
  -- FK
  CONSTRAINT `fk_pagoserr_tenant` FOREIGN KEY (`tenant_id`) 
    REFERENCES `tenants` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
    
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='Registro de transacciones fallidas o con error';


-- ============================================================
-- Tabla: pagos_intento_log
-- Registro de TODOS los intentos (exitosos o no)
-- Útil para detectar patrones, fraude, rate limiting
-- ============================================================
DROP TABLE IF EXISTS `pagos_intento_log`;
CREATE TABLE `pagos_intento_log` (
  `id` BIGINT NOT NULL AUTO_INCREMENT,
  
  -- Identificación
  `tenant_id` INT NOT NULL,
  `gateway` VARCHAR(20) NOT NULL,
  `ambiente` VARCHAR(10) NOT NULL DEFAULT 'test',
  
  -- Request info
  `endpoint` VARCHAR(200) NOT NULL COMMENT '/api/banamex/pay, /api/stripe/charge, etc',
  `metodo_http` VARCHAR(10) NOT NULL COMMENT 'POST, GET, PUT',
  `tipo_operacion` VARCHAR(30) NOT NULL,
  
  -- IDs
  `order_id` VARCHAR(100) DEFAULT NULL,
  `transaction_id` VARCHAR(100) DEFAULT NULL,
  
  -- Cliente
  `ip_cliente` VARCHAR(45) NOT NULL,
  `user_agent` VARCHAR(500) DEFAULT NULL,
  `api_key_last4` VARCHAR(8) DEFAULT NULL COMMENT 'Últimos 4 del API Key usado',
  
  -- Tarjeta (enmascarada)
  `tarjeta_last4` VARCHAR(4) DEFAULT NULL,
  `tarjeta_bin` VARCHAR(6) DEFAULT NULL,
  
  -- Resultado
  `exitoso` TINYINT(1) NOT NULL DEFAULT 0,
  `http_status_code` INT NOT NULL,
  `error_tipo` VARCHAR(50) DEFAULT NULL,
  
  -- Monto
  `monto` DECIMAL(18,2) DEFAULT NULL,
  `moneda` VARCHAR(3) DEFAULT 'MXN',
  
  -- Performance
  `duracion_ms` INT DEFAULT NULL,
  
  -- Metadata mínima (para no duplicar info)
  `metadata_json` TEXT COMMENT 'Headers adicionales relevantes',
  
  -- Auditoría
  `creado_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  
  PRIMARY KEY (`id`),
  
  -- Índices para detección de patrones
  INDEX `ix_intento_tenant_created` (`tenant_id`, `creado_utc`),
  INDEX `ix_intento_ip_created` (`ip_cliente`, `creado_utc`),
  INDEX `ix_intento_tarjeta_created` (`tarjeta_last4`, `creado_utc`),
  INDEX `ix_intento_exitoso` (`exitoso`, `creado_utc`),
  INDEX `ix_intento_gateway` (`gateway`, `creado_utc`),
  INDEX `ix_intento_created` (`creado_utc`),
  INDEX `ix_intento_endpoint` (`endpoint`),
  INDEX `ix_intento_apikey` (`api_key_last4`, `creado_utc`),
  
  -- FK
  CONSTRAINT `fk_intento_tenant` FOREIGN KEY (`tenant_id`) 
    REFERENCES `tenants` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
    
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='Registro de todos los intentos de pago (para rate limiting, fraude, analytics)';


-- ============================================================
-- Tabla: pagos_webhook_log
-- Registro de webhooks recibidos de los gateways
-- ============================================================
DROP TABLE IF EXISTS `pagos_webhook_log`;
CREATE TABLE `pagos_webhook_log` (
  `id` BIGINT NOT NULL AUTO_INCREMENT,
  
  -- Identificación
  `gateway` VARCHAR(20) NOT NULL,
  `evento_tipo` VARCHAR(50) NOT NULL COMMENT 'payment.captured, payment.failed, refund.completed',
  
  -- IDs relacionados
  `tenant_id` INT DEFAULT NULL COMMENT 'Puede ser NULL si no se pudo determinar',
  `order_id` VARCHAR(100) DEFAULT NULL,
  `transaction_id` VARCHAR(100) DEFAULT NULL,
  `gateway_event_id` VARCHAR(150) DEFAULT NULL COMMENT 'ID del evento en el gateway',
  
  -- Request recibido
  `ip_origen` VARCHAR(45) NOT NULL,
  `user_agent` VARCHAR(500) DEFAULT NULL,
  `headers_json` TEXT COMMENT 'Headers relevantes del webhook',
  `payload_json` LONGTEXT NOT NULL COMMENT 'Body completo del webhook',
  
  -- Validación
  `firma_valida` TINYINT(1) DEFAULT NULL COMMENT 'Si la firma HMAC es válida',
  `firma_recibida` VARCHAR(500) DEFAULT NULL,
  
  -- Procesamiento
  `procesado` TINYINT(1) NOT NULL DEFAULT 0,
  `procesado_utc` DATETIME(6) DEFAULT NULL,
  `procesado_exitoso` TINYINT(1) DEFAULT NULL,
  `error_proceso` TEXT DEFAULT NULL COMMENT 'Error al procesar el webhook',
  
  -- Auditoría
  `creado_utc` DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  
  PRIMARY KEY (`id`),
  
  -- Índices
  INDEX `ix_webhook_gateway_created` (`gateway`, `creado_utc`),
  INDEX `ix_webhook_evento` (`evento_tipo`),
  INDEX `ix_webhook_tenant` (`tenant_id`, `creado_utc`),
  INDEX `ix_webhook_order` (`order_id`),
  INDEX `ix_webhook_procesado` (`procesado`, `creado_utc`),
  INDEX `ix_webhook_created` (`creado_utc`),
  INDEX `ix_webhook_gateway_event` (`gateway_event_id`),
  
  -- FK (puede ser NULL)
  CONSTRAINT `fk_webhook_tenant` FOREIGN KEY (`tenant_id`) 
    REFERENCES `tenants` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
    
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
COMMENT='Registro de webhooks recibidos de los gateways';


-- ============================================================
-- Vistas útiles para reporting
-- ============================================================

-- Vista: Transacciones del día por gateway
CREATE OR REPLACE VIEW `v_pagos_hoy_por_gateway` AS
SELECT 
    gateway,
    COUNT(*) AS total_transacciones,
    SUM(monto) AS monto_total,
    AVG(monto) AS monto_promedio,
    AVG(duracion_ms) AS duracion_promedio_ms,
    COUNT(DISTINCT tenant_id) AS tenants_unicos
FROM pagos_ok_log
WHERE DATE(creado_utc) = CURDATE()
GROUP BY gateway;

-- Vista: Tasa de error por gateway (últimas 24h)
CREATE OR REPLACE VIEW `v_tasa_error_24h` AS
SELECT 
    g.gateway,
    IFNULL(ok.total, 0) AS exitosos,
    IFNULL(err.total, 0) AS errores,
    IFNULL(ok.total, 0) + IFNULL(err.total, 0) AS total,
    ROUND(
        IFNULL(err.total, 0) / NULLIF(IFNULL(ok.total, 0) + IFNULL(err.total, 0), 0) * 100, 
        2
    ) AS tasa_error_pct
FROM (
    SELECT DISTINCT gateway FROM pagos_ok_log
    UNION
    SELECT DISTINCT gateway FROM pagos_error_log
) g
LEFT JOIN (
    SELECT gateway, COUNT(*) AS total
    FROM pagos_ok_log
    WHERE creado_utc >= DATE_SUB(NOW(), INTERVAL 24 HOUR)
    GROUP BY gateway
) ok ON g.gateway = ok.gateway
LEFT JOIN (
    SELECT gateway, COUNT(*) AS total
    FROM pagos_error_log
    WHERE creado_utc >= DATE_SUB(NOW(), INTERVAL 24 HOUR)
    GROUP BY gateway
) err ON g.gateway = err.gateway;

-- Vista: Top errores por tipo (última semana)
CREATE OR REPLACE VIEW `v_top_errores_semana` AS
SELECT 
    gateway,
    error_tipo,
    error_codigo,
    COUNT(*) AS ocurrencias,
    COUNT(DISTINCT tenant_id) AS tenants_afectados,
    MIN(creado_utc) AS primer_ocurrencia,
    MAX(creado_utc) AS ultima_ocurrencia
FROM pagos_error_log
WHERE creado_utc >= DATE_SUB(NOW(), INTERVAL 7 DAY)
GROUP BY gateway, error_tipo, error_codigo
ORDER BY ocurrencias DESC
LIMIT 50;

-- Vista: Estadísticas por tenant (último mes)
CREATE OR REPLACE VIEW `v_stats_tenant_mes` AS
SELECT 
    t.id AS tenant_id,
    t.nombre AS tenant_nombre,
    COUNT(DISTINCT p.gateway) AS gateways_usados,
    COUNT(p.id) AS total_transacciones,
    SUM(p.monto) AS monto_total,
    AVG(p.monto) AS ticket_promedio,
    (SELECT COUNT(*) FROM pagos_error_log e 
     WHERE e.tenant_id = t.id 
     AND e.creado_utc >= DATE_SUB(NOW(), INTERVAL 30 DAY)) AS errores,
    ROUND(
        (SELECT COUNT(*) FROM pagos_error_log e 
         WHERE e.tenant_id = t.id 
         AND e.creado_utc >= DATE_SUB(NOW(), INTERVAL 30 DAY)) 
        / NULLIF(COUNT(p.id), 0) * 100, 
        2
    ) AS tasa_error_pct
FROM tenants t
LEFT JOIN pagos_ok_log p ON t.id = p.tenant_id 
    AND p.creado_utc >= DATE_SUB(NOW(), INTERVAL 30 DAY)
GROUP BY t.id, t.nombre;

-- ============================================================
-- Índices adicionales para performance
-- ============================================================

-- Para búsquedas por rango de fechas y monto
ALTER TABLE pagos_ok_log 
    ADD INDEX `ix_pagosok_fecha_monto` (`creado_utc`, `monto`);

-- Para reportes de fraude (múltiples transacciones desde misma IP)
ALTER TABLE pagos_intento_log 
    ADD INDEX `ix_intento_ip_tarjeta` (`ip_cliente`, `tarjeta_last4`, `creado_utc`);

-- Para análisis de performance
ALTER TABLE pagos_ok_log 
    ADD INDEX `ix_pagosok_duracion` (`gateway`, `duracion_ms`);

-- ============================================================
-- Procedimientos almacenados útiles
-- ============================================================

DELIMITER $$

-- Obtener estadísticas de un tenant
CREATE PROCEDURE `sp_stats_tenant`(IN p_tenant_id INT, IN p_dias INT)
BEGIN
    DECLARE v_fecha_desde DATETIME;
    SET v_fecha_desde = DATE_SUB(NOW(), INTERVAL p_dias DAY);
    
    SELECT 
        'exitosos' AS tipo,
        gateway,
        COUNT(*) AS cantidad,
        SUM(monto) AS monto_total,
        AVG(monto) AS monto_promedio,
        AVG(duracion_ms) AS duracion_promedio_ms
    FROM pagos_ok_log
    WHERE tenant_id = p_tenant_id
        AND creado_utc >= v_fecha_desde
    GROUP BY gateway
    
    UNION ALL
    
    SELECT 
        'errores' AS tipo,
        gateway,
        COUNT(*) AS cantidad,
        SUM(IFNULL(monto, 0)) AS monto_total,
        AVG(IFNULL(monto, 0)) AS monto_promedio,
        AVG(duracion_ms) AS duracion_promedio_ms
    FROM pagos_error_log
    WHERE tenant_id = p_tenant_id
        AND creado_utc >= v_fecha_desde
    GROUP BY gateway;
END$$

-- Detectar posible fraude (múltiples intentos fallidos)
CREATE PROCEDURE `sp_detectar_fraude`(IN p_minutos INT, IN p_min_intentos INT)
BEGIN
    DECLARE v_fecha_desde DATETIME;
    SET v_fecha_desde = DATE_SUB(NOW(), INTERVAL p_minutos MINUTE);
    
    SELECT 
        ip_cliente,
        tarjeta_last4,
        tarjeta_bin,
        COUNT(*) AS intentos_fallidos,
        COUNT(DISTINCT tenant_id) AS tenants_diferentes,
        MIN(creado_utc) AS primer_intento,
        MAX(creado_utc) AS ultimo_intento,
        SUM(monto) AS monto_total_intentado
    FROM pagos_intento_log
    WHERE creado_utc >= v_fecha_desde
        AND exitoso = 0
    GROUP BY ip_cliente, tarjeta_last4, tarjeta_bin
    HAVING COUNT(*) >= p_min_intentos
    ORDER BY intentos_fallidos DESC;
END$$

DELIMITER ;

-- ============================================================
-- Datos de prueba (opcional - comentar en producción)
-- ============================================================

/*
-- Insertar algunos logs de ejemplo
INSERT INTO pagos_ok_log (tenant_id, gateway, order_id, transaction_id, tipo_operacion, monto, tarjeta_tipo, tarjeta_last4, gateway_code, result_code, ip_cliente, duracion_ms)
VALUES 
(1, 'banamex', 'ORD-001', 'TXN-001', 'PAY', 100.50, 'VISA', '1234', 'APPROVED', 'SUCCESS', '192.168.1.1', 1250),
(1, 'stripe', 'ORD-002', 'TXN-002', 'PAY', 250.00, 'MASTERCARD', '5678', 'APPROVED', 'SUCCESS', '192.168.1.2', 890);

INSERT INTO pagos_error_log (tenant_id, gateway, order_id, tipo_operacion, monto, error_tipo, error_codigo, error_mensaje, ip_cliente)
VALUES 
(1, 'banamex', 'ORD-003', 'PAY', 500.00, 'DECLINED', 'INSUFFICIENT_FUNDS', 'Fondos insuficientes', '192.168.1.1');
*/

-- ============================================================
-- FIN DEL SCRIPT
-- ============================================================
