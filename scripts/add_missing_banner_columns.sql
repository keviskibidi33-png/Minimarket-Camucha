-- Script para agregar las columnas faltantes en la tabla Banners
USE MinimarketDB;
GO

SET NOCOUNT ON;
PRINT '=== AGREGANDO COLUMNAS FALTANTES A BANNERS ===';
PRINT '';

-- Verificar si la tabla existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Banners')
BEGIN
    PRINT 'ERROR: La tabla Banners no existe.';
    RETURN;
END

-- Agregar Posicion (columna crítica que falta)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'Posicion')
BEGIN
    ALTER TABLE Banners ADD Posicion INT NOT NULL DEFAULT 0;
    PRINT '✓ Posicion agregada (INT, DEFAULT 0)';
END
ELSE
BEGIN
    PRINT '✓ Posicion ya existe';
END

-- Agregar AnchoMaximo
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'AnchoMaximo')
BEGIN
    ALTER TABLE Banners ADD AnchoMaximo INT NULL;
    PRINT '✓ AnchoMaximo agregada';
END
ELSE
BEGIN
    PRINT '✓ AnchoMaximo ya existe';
END

-- Agregar AltoMaximo
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'AltoMaximo')
BEGIN
    ALTER TABLE Banners ADD AltoMaximo INT NULL;
    PRINT '✓ AltoMaximo agregada';
END
ELSE
BEGIN
    PRINT '✓ AltoMaximo ya existe';
END

-- Agregar ClasesCss
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'ClasesCss')
BEGIN
    ALTER TABLE Banners ADD ClasesCss NVARCHAR(500) NULL;
    PRINT '✓ ClasesCss agregada';
END
ELSE
BEGIN
    PRINT '✓ ClasesCss ya existe';
END

-- Agregar SoloMovil
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'SoloMovil')
BEGIN
    ALTER TABLE Banners ADD SoloMovil BIT NOT NULL DEFAULT 0;
    PRINT '✓ SoloMovil agregada';
END
ELSE
BEGIN
    PRINT '✓ SoloMovil ya existe';
END

-- Agregar SoloDesktop
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'SoloDesktop')
BEGIN
    ALTER TABLE Banners ADD SoloDesktop BIT NOT NULL DEFAULT 0;
    PRINT '✓ SoloDesktop agregada';
END
ELSE
BEGIN
    PRINT '✓ SoloDesktop ya existe';
END

-- Agregar MaxVisualizaciones
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'MaxVisualizaciones')
BEGIN
    ALTER TABLE Banners ADD MaxVisualizaciones INT NULL;
    PRINT '✓ MaxVisualizaciones agregada';
END
ELSE
BEGIN
    PRINT '✓ MaxVisualizaciones ya existe';
END

-- Agregar VisualizacionesActuales
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'VisualizacionesActuales')
BEGIN
    ALTER TABLE Banners ADD VisualizacionesActuales INT NOT NULL DEFAULT 0;
    PRINT '✓ VisualizacionesActuales agregada';
END
ELSE
BEGIN
    PRINT '✓ VisualizacionesActuales ya existe';
END

-- Crear índices si no existen
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Tipo' AND object_id = OBJECT_ID('Banners'))
BEGIN
    CREATE INDEX IX_Banners_Tipo ON Banners(Tipo);
    PRINT '✓ Índice IX_Banners_Tipo creado';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Posicion' AND object_id = OBJECT_ID('Banners'))
BEGIN
    CREATE INDEX IX_Banners_Posicion ON Banners(Posicion);
    PRINT '✓ Índice IX_Banners_Posicion creado';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Orden' AND object_id = OBJECT_ID('Banners'))
BEGIN
    CREATE INDEX IX_Banners_Orden ON Banners(Orden);
    PRINT '✓ Índice IX_Banners_Orden creado';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Activo_Fechas' AND object_id = OBJECT_ID('Banners'))
BEGIN
    CREATE INDEX IX_Banners_Activo_Fechas ON Banners(Activo, FechaInicio, FechaFin);
    PRINT '✓ Índice IX_Banners_Activo_Fechas creado';
END

-- Verificar estructura final
PRINT '';
PRINT '=== ESTRUCTURA FINAL DE LA TABLA ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Banners'
ORDER BY ORDINAL_POSITION;

PRINT '';
PRINT '=== COMPLETADO ===';
PRINT 'Todas las columnas necesarias están presentes.';
GO

