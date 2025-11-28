-- Script SIMPLE y SEGURO para corregir la tabla Banners
-- Usa SQL dinámico para evitar errores de compilación
USE MinimarketDB;
GO

SET NOCOUNT ON;
PRINT '=== CORRIGIENDO TABLA BANNERS (VERSIÓN SIMPLE) ===';
PRINT '';

-- Verificar si la tabla existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Banners')
BEGIN
    PRINT 'ERROR: La tabla Banners no existe. Ejecuta primero create_banners_table.sql';
    RETURN;
END

-- PASO 1: Limpiar cualquier columna temporal que pueda quedar
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'PosicionTemp')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'Posicion')
    BEGIN
        EXEC sp_rename 'Banners.PosicionTemp', 'Posicion', 'COLUMN';
        PRINT '✓ PosicionTemp renombrada a Posicion';
    END
    ELSE
    BEGIN
        EXEC('ALTER TABLE Banners DROP COLUMN PosicionTemp');
        PRINT '✓ Columna temporal PosicionTemp eliminada';
    END
END

-- PASO 2: Renombrar columnas de inglés a español
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'Title')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'Titulo')
BEGIN
    EXEC sp_rename 'Banners.Title', 'Titulo', 'COLUMN';
    PRINT '✓ Title -> Titulo';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'Description')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'Descripcion')
BEGIN
    EXEC sp_rename 'Banners.Description', 'Descripcion', 'COLUMN';
    PRINT '✓ Description -> Descripcion';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'ImageUrl')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'ImagenUrl')
BEGIN
    EXEC sp_rename 'Banners.ImageUrl', 'ImagenUrl', 'COLUMN';
    PRINT '✓ ImageUrl -> ImagenUrl';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'LinkUrl')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'UrlDestino')
BEGIN
    EXEC sp_rename 'Banners.LinkUrl', 'UrlDestino', 'COLUMN';
    PRINT '✓ LinkUrl -> UrlDestino';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'DisplayOrder')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'Orden')
BEGIN
    EXEC sp_rename 'Banners.DisplayOrder', 'Orden', 'COLUMN';
    PRINT '✓ DisplayOrder -> Orden';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'IsActive')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'Activo')
BEGIN
    EXEC sp_rename 'Banners.IsActive', 'Activo', 'COLUMN';
    PRINT '✓ IsActive -> Activo';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'StartDate')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'FechaInicio')
BEGIN
    EXEC sp_rename 'Banners.StartDate', 'FechaInicio', 'COLUMN';
    PRINT '✓ StartDate -> FechaInicio';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'EndDate')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'FechaFin')
BEGIN
    EXEC sp_rename 'Banners.EndDate', 'FechaFin', 'COLUMN';
    PRINT '✓ EndDate -> FechaFin';
END

-- PASO 3: Manejar Position/Posicion usando SQL dinámico
DECLARE @PositionExists BIT = 0;
DECLARE @PosicionExists BIT = 0;
DECLARE @PositionType NVARCHAR(50) = '';

-- Verificar qué columnas existen
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'Position')
BEGIN
    SET @PositionExists = 1;
    SELECT @PositionType = DATA_TYPE 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Banners' AND COLUMN_NAME = 'Position';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'Posicion')
BEGIN
    SET @PosicionExists = 1;
    IF @PositionType = ''
    BEGIN
        SELECT @PositionType = DATA_TYPE 
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = 'Banners' AND COLUMN_NAME = 'Posicion';
    END
END

-- Si Position existe y Posicion no, convertir
IF @PositionExists = 1 AND @PosicionExists = 0
BEGIN
    IF @PositionType = 'nvarchar'
    BEGIN
        PRINT 'Convirtiendo Position (NVARCHAR) a Posicion (INT)...';
        
        -- Crear PosicionTemp
        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'PosicionTemp')
        BEGIN
            EXEC('ALTER TABLE Banners ADD PosicionTemp INT NOT NULL DEFAULT 0');
            PRINT '  - Columna PosicionTemp creada';
        END
        
        -- Convertir datos usando SQL dinámico
        DECLARE @sql NVARCHAR(MAX) = '
            UPDATE Banners 
            SET PosicionTemp = CASE 
                WHEN LOWER(Position) LIKE ''%top%'' OR LOWER(Position) LIKE ''%arriba%'' THEN 0
                WHEN LOWER(Position) LIKE ''%middle%'' OR LOWER(Position) LIKE ''%medio%'' THEN 1
                WHEN LOWER(Position) LIKE ''%bottom%'' OR LOWER(Position) LIKE ''%abajo%'' THEN 2
                WHEN LOWER(Position) LIKE ''%left%'' OR LOWER(Position) LIKE ''%izquierda%'' THEN 3
                WHEN LOWER(Position) LIKE ''%right%'' OR LOWER(Position) LIKE ''%derecha%'' THEN 4
                WHEN LOWER(Position) LIKE ''%center%'' OR LOWER(Position) LIKE ''%centro%'' THEN 5
                ELSE 0
            END';
        
        EXEC sp_executesql @sql;
        PRINT '  - Datos convertidos';
        
        -- Eliminar Position
        EXEC('ALTER TABLE Banners DROP COLUMN Position');
        PRINT '  - Columna Position eliminada';
        
        -- Renombrar PosicionTemp a Posicion
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'PosicionTemp')
        BEGIN
            EXEC sp_rename 'Banners.PosicionTemp', 'Posicion', 'COLUMN';
            PRINT '  - PosicionTemp renombrada a Posicion';
        END
        
        PRINT '✓ Position (NVARCHAR) -> Posicion (INT)';
    END
    ELSE
    BEGIN
        -- Es INT, solo renombrar
        EXEC sp_rename 'Banners.Position', 'Posicion', 'COLUMN';
        PRINT '✓ Position (INT) -> Posicion';
    END
END
ELSE IF @PosicionExists = 1 AND @PositionType = 'nvarchar'
BEGIN
    -- Posicion existe pero es NVARCHAR, convertir a INT
    PRINT 'Convirtiendo Posicion (NVARCHAR) a Posicion (INT)...';
    
    -- Crear PosicionTemp
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'PosicionTemp')
    BEGIN
        EXEC('ALTER TABLE Banners ADD PosicionTemp INT NOT NULL DEFAULT 0');
        PRINT '  - Columna PosicionTemp creada';
    END
    
    -- Convertir datos usando SQL dinámico
    DECLARE @sql2 NVARCHAR(MAX) = '
        UPDATE Banners 
        SET PosicionTemp = CASE 
            WHEN LOWER(Posicion) LIKE ''%top%'' OR LOWER(Posicion) LIKE ''%arriba%'' THEN 0
            WHEN LOWER(Posicion) LIKE ''%middle%'' OR LOWER(Posicion) LIKE ''%medio%'' THEN 1
            WHEN LOWER(Posicion) LIKE ''%bottom%'' OR LOWER(Posicion) LIKE ''%abajo%'' THEN 2
            WHEN LOWER(Posicion) LIKE ''%left%'' OR LOWER(Posicion) LIKE ''%izquierda%'' THEN 3
            WHEN LOWER(Posicion) LIKE ''%right%'' OR LOWER(Posicion) LIKE ''%derecha%'' THEN 4
            WHEN LOWER(Posicion) LIKE ''%center%'' OR LOWER(Posicion) LIKE ''%centro%'' THEN 5
            ELSE 0
        END';
    
    EXEC sp_executesql @sql2;
    PRINT '  - Datos convertidos';
    
    -- Eliminar Posicion
    EXEC('ALTER TABLE Banners DROP COLUMN Posicion');
    PRINT '  - Columna Posicion (NVARCHAR) eliminada';
    
    -- Renombrar PosicionTemp a Posicion
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'PosicionTemp')
    BEGIN
        EXEC sp_rename 'Banners.PosicionTemp', 'Posicion', 'COLUMN';
        PRINT '  - PosicionTemp renombrada a Posicion';
    END
    
    PRINT '✓ Posicion (NVARCHAR) -> Posicion (INT)';
END
ELSE IF @PosicionExists = 1
BEGIN
    PRINT '✓ Posicion ya existe y es INT';
END

-- PASO 4: Agregar columnas faltantes
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'AbrirEnNuevaVentana')
BEGIN
    EXEC('ALTER TABLE Banners ADD AbrirEnNuevaVentana BIT NOT NULL DEFAULT 0');
    PRINT '✓ AbrirEnNuevaVentana agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'Tipo')
BEGIN
    EXEC('ALTER TABLE Banners ADD Tipo INT NOT NULL DEFAULT 0');
    PRINT '✓ Tipo agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'AnchoMaximo')
BEGIN
    EXEC('ALTER TABLE Banners ADD AnchoMaximo INT NULL');
    PRINT '✓ AnchoMaximo agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'AltoMaximo')
BEGIN
    EXEC('ALTER TABLE Banners ADD AltoMaximo INT NULL');
    PRINT '✓ AltoMaximo agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'ClasesCss')
BEGIN
    EXEC('ALTER TABLE Banners ADD ClasesCss NVARCHAR(500) NULL');
    PRINT '✓ ClasesCss agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'SoloMovil')
BEGIN
    EXEC('ALTER TABLE Banners ADD SoloMovil BIT NOT NULL DEFAULT 0');
    PRINT '✓ SoloMovil agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'SoloDesktop')
BEGIN
    EXEC('ALTER TABLE Banners ADD SoloDesktop BIT NOT NULL DEFAULT 0');
    PRINT '✓ SoloDesktop agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'MaxVisualizaciones')
BEGIN
    EXEC('ALTER TABLE Banners ADD MaxVisualizaciones INT NULL');
    PRINT '✓ MaxVisualizaciones agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'VisualizacionesActuales')
BEGIN
    EXEC('ALTER TABLE Banners ADD VisualizacionesActuales INT NOT NULL DEFAULT 0');
    PRINT '✓ VisualizacionesActuales agregada';
END

-- PASO 5: Corregir tamaño de Descripcion
IF EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Banners' 
    AND COLUMN_NAME = 'Descripcion' 
    AND CHARACTER_MAXIMUM_LENGTH = 500
)
BEGIN
    EXEC('ALTER TABLE Banners ALTER COLUMN Descripcion NVARCHAR(1000) NULL');
    PRINT '✓ Descripcion actualizada a NVARCHAR(1000)';
END

-- PASO 6: Limpiar y recrear índices
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_DisplayOrder' AND object_id = OBJECT_ID('Banners'))
BEGIN
    EXEC('DROP INDEX IX_Banners_DisplayOrder ON Banners');
    PRINT '✓ Índice IX_Banners_DisplayOrder eliminado';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Position' AND object_id = OBJECT_ID('Banners'))
BEGIN
    EXEC('DROP INDEX IX_Banners_Position ON Banners');
    PRINT '✓ Índice IX_Banners_Position eliminado';
END

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_IsActive' AND object_id = OBJECT_ID('Banners'))
BEGIN
    EXEC('DROP INDEX IX_Banners_IsActive ON Banners');
    PRINT '✓ Índice IX_Banners_IsActive eliminado';
END

-- Crear índices correctos
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Tipo' AND object_id = OBJECT_ID('Banners'))
BEGIN
    EXEC('CREATE INDEX IX_Banners_Tipo ON Banners(Tipo)');
    PRINT '✓ Índice IX_Banners_Tipo creado';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Posicion' AND object_id = OBJECT_ID('Banners'))
BEGIN
    EXEC('CREATE INDEX IX_Banners_Posicion ON Banners(Posicion)');
    PRINT '✓ Índice IX_Banners_Posicion creado';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Orden' AND object_id = OBJECT_ID('Banners'))
BEGIN
    EXEC('CREATE INDEX IX_Banners_Orden ON Banners(Orden)');
    PRINT '✓ Índice IX_Banners_Orden creado';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Activo_Fechas' AND object_id = OBJECT_ID('Banners'))
BEGIN
    EXEC('CREATE INDEX IX_Banners_Activo_Fechas ON Banners(Activo, FechaInicio, FechaFin)');
    PRINT '✓ Índice IX_Banners_Activo_Fechas creado';
END

-- PASO 7: Verificar estructura final
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
PRINT '=== CORRECCIÓN COMPLETA ===';
PRINT 'La tabla Banners está lista para usar.';
GO

