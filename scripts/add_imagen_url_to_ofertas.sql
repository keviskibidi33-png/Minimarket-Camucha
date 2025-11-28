-- Script para agregar la columna ImagenUrl a la tabla Ofertas
USE MinimarketDB;
GO

-- Verificar si la columna ya existe
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('Ofertas') 
    AND name = 'ImagenUrl'
)
BEGIN
    ALTER TABLE Ofertas
    ADD ImagenUrl NVARCHAR(500) NULL;
    
    PRINT 'Columna ImagenUrl agregada exitosamente a la tabla Ofertas';
END
ELSE
BEGIN
    PRINT 'La columna ImagenUrl ya existe en la tabla Ofertas';
END
GO

-- Verificar la estructura actualizada
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Ofertas'
AND COLUMN_NAME = 'ImagenUrl';
GO

PRINT '=== MIGRACIÓN COMPLETA ===';
GO

