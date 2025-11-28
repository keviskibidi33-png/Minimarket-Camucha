-- Script para aplicar la migración AddMostrarEnNavbarToPage
-- Ejecutar este script en SQL Server Management Studio o en la base de datos

USE MinimarketDB;
GO

-- Verificar si la columna ya existe
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('Pages') 
    AND name = 'MostrarEnNavbar'
)
BEGIN
    -- Agregar la columna MostrarEnNavbar
    ALTER TABLE Pages
    ADD MostrarEnNavbar BIT NOT NULL DEFAULT 0;
    
    PRINT 'Columna MostrarEnNavbar agregada exitosamente a la tabla Pages';
END
ELSE
BEGIN
    PRINT 'La columna MostrarEnNavbar ya existe en la tabla Pages';
END
GO

-- Verificar que la columna se agregó correctamente
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Pages'
AND COLUMN_NAME = 'MostrarEnNavbar';
GO

