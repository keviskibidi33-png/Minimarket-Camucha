-- Script para crear la tabla Banners
USE MinimarketDB;
GO

-- Verificar si la tabla ya existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Banners')
BEGIN
    CREATE TABLE Banners (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Titulo NVARCHAR(200) NOT NULL,
        Descripcion NVARCHAR(1000) NULL,
        ImagenUrl NVARCHAR(500) NOT NULL,
        UrlDestino NVARCHAR(500) NULL,
        AbrirEnNuevaVentana BIT NOT NULL DEFAULT 0,
        Tipo INT NOT NULL DEFAULT 0,
        Posicion INT NOT NULL DEFAULT 0,
        FechaInicio DATETIME2 NULL,
        FechaFin DATETIME2 NULL,
        Activo BIT NOT NULL DEFAULT 1,
        Orden INT NOT NULL DEFAULT 0,
        AnchoMaximo INT NULL,
        AltoMaximo INT NULL,
        ClasesCss NVARCHAR(500) NULL,
        SoloMovil BIT NOT NULL DEFAULT 0,
        SoloDesktop BIT NOT NULL DEFAULT 0,
        MaxVisualizaciones INT NULL,
        VisualizacionesActuales INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL
    );

    -- Crear índices para mejorar el rendimiento
    CREATE INDEX IX_Banners_Tipo ON Banners(Tipo);
    CREATE INDEX IX_Banners_Posicion ON Banners(Posicion);
    CREATE INDEX IX_Banners_Orden ON Banners(Orden);
    CREATE INDEX IX_Banners_Activo_Fechas ON Banners(Activo, FechaInicio, FechaFin);

    PRINT 'Tabla Banners creada exitosamente';
END
ELSE
BEGIN
    PRINT 'La tabla Banners ya existe';
END
GO

-- Verificar la estructura de la tabla
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Banners'
ORDER BY ORDINAL_POSITION;
GO

PRINT '=== MIGRACIÓN COMPLETA ===';
PRINT 'La tabla Banners está lista para usar.';
GO

