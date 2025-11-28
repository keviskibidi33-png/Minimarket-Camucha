-- Script para corregir definitivamente el valor de IGV
-- Ejecutar en SSMS conectado a POSGRES\SQLEXPRESS

USE MinimarketDB;
GO

-- Ver el estado actual
SELECT [Key], Value, IsActive, UpdatedAt
FROM SystemSettings
WHERE [Key] IN ('apply_igv_to_cart', 'igv_rate');
GO

-- Corregir apply_igv_to_cart - asegurar que sea exactamente 'true'
UPDATE SystemSettings
SET Value = 'true',
    UpdatedAt = GETDATE()
WHERE [Key] = 'apply_igv_to_cart';
GO

-- Asegurar que igv_rate tenga un valor
UPDATE SystemSettings
SET Value = '0.18',
    UpdatedAt = GETDATE()
WHERE [Key] = 'igv_rate'
  AND (Value IS NULL OR Value = '' OR LTRIM(RTRIM(Value)) = '');
GO

-- Verificar que se actualizó correctamente
SELECT 
    [Key], 
    Value, 
    LEN(Value) AS Longitud,
    IsActive,
    UpdatedAt
FROM SystemSettings
WHERE [Key] IN ('apply_igv_to_cart', 'igv_rate')
ORDER BY [Key];
GO

-- Verificar que no haya espacios
SELECT 
    [Key],
    Value,
    CASE 
        WHEN Value = 'true' THEN '✅ CORRECTO'
        WHEN Value = 'false' THEN '❌ DESACTIVADO'
        WHEN Value = '' OR Value IS NULL THEN '⚠️ VACÍO'
        ELSE '⚠️ VALOR DESCONOCIDO: ' + Value
    END AS Estado
FROM SystemSettings
WHERE [Key] = 'apply_igv_to_cart';
GO

