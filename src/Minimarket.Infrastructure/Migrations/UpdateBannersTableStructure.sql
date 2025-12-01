-- Script SQL para actualizar la estructura de la tabla Banners
-- Ejecutar este script directamente en la base de datos si no puedes usar dotnet ef

BEGIN TRANSACTION;

-- Renombrar columnas existentes de inglés a español
EXEC sp_rename 'Banners.Title', 'Titulo', 'COLUMN';
EXEC sp_rename 'Banners.Description', 'Descripcion', 'COLUMN';
EXEC sp_rename 'Banners.LinkUrl', 'UrlDestino', 'COLUMN';
EXEC sp_rename 'Banners.DisplayOrder', 'Orden', 'COLUMN';
EXEC sp_rename 'Banners.IsActive', 'Activo', 'COLUMN';
EXEC sp_rename 'Banners.StartDate', 'FechaInicio', 'COLUMN';
EXEC sp_rename 'Banners.EndDate', 'FechaFin', 'COLUMN';

-- Eliminar el índice antiguo de Position
DROP INDEX IF EXISTS IX_Banners_Position ON Banners;

-- Eliminar la columna Position (string) - primero eliminar el índice si existe
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Position' AND object_id = OBJECT_ID('Banners'))
BEGIN
    DROP INDEX IX_Banners_Position ON Banners;
END

-- Eliminar la columna Position
ALTER TABLE Banners DROP COLUMN Position;

-- Agregar nuevas columnas
ALTER TABLE Banners ADD AbrirEnNuevaVentana BIT NOT NULL DEFAULT 0;
ALTER TABLE Banners ADD Tipo INT NOT NULL DEFAULT 0;
ALTER TABLE Banners ADD Posicion INT NOT NULL DEFAULT 0;
ALTER TABLE Banners ADD AnchoMaximo INT NULL;
ALTER TABLE Banners ADD AltoMaximo INT NULL;
ALTER TABLE Banners ADD ClasesCss NVARCHAR(500) NULL;
ALTER TABLE Banners ADD SoloMovil BIT NOT NULL DEFAULT 0;
ALTER TABLE Banners ADD SoloDesktop BIT NOT NULL DEFAULT 0;
ALTER TABLE Banners ADD MaxVisualizaciones INT NULL;
ALTER TABLE Banners ADD VisualizacionesActuales INT NOT NULL DEFAULT 0;
ALTER TABLE Banners ADD IsDeleted BIT NOT NULL DEFAULT 0;
ALTER TABLE Banners ADD DeletedAt DATETIME2 NULL;

-- Renombrar índices existentes
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_DisplayOrder' AND object_id = OBJECT_ID('Banners'))
BEGIN
    EXEC sp_rename 'Banners.IX_Banners_DisplayOrder', 'IX_Banners_Orden', 'INDEX';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_IsActive' AND object_id = OBJECT_ID('Banners'))
BEGIN
    EXEC sp_rename 'Banners.IX_Banners_IsActive', 'IX_Banners_Activo', 'INDEX';
END

-- Crear nuevos índices
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Tipo' AND object_id = OBJECT_ID('Banners'))
BEGIN
    CREATE INDEX IX_Banners_Tipo ON Banners(Tipo);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Posicion' AND object_id = OBJECT_ID('Banners'))
BEGIN
    CREATE INDEX IX_Banners_Posicion ON Banners(Posicion);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Activo_Fechas' AND object_id = OBJECT_ID('Banners'))
BEGIN
    CREATE INDEX IX_Banners_Activo_Fechas ON Banners(Activo, FechaInicio, FechaFin);
END

COMMIT TRANSACTION;

