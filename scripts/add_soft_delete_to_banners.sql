-- Script para agregar Soft Delete a la tabla Banners
USE MinimarketDB;
GO

SET NOCOUNT ON;
PRINT '=== AGREGANDO SOFT DELETE A BANNERS ===';
PRINT '';

-- Verificar si la tabla existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Banners')
BEGIN
    PRINT 'ERROR: La tabla Banners no existe.';
    RETURN;
END

-- Agregar IsDeleted si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE Banners ADD IsDeleted BIT NOT NULL DEFAULT 0;
    PRINT '✓ IsDeleted agregada (BIT, DEFAULT 0)';
END
ELSE
BEGIN
    PRINT '✓ IsDeleted ya existe';
END

-- Agregar DeletedAt si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE Banners ADD DeletedAt DATETIME2 NULL;
    PRINT '✓ DeletedAt agregada (DATETIME2, NULL)';
END
ELSE
BEGIN
    PRINT '✓ DeletedAt ya existe';
END

-- Crear índice para mejorar consultas que filtran por IsDeleted
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_IsDeleted' AND object_id = OBJECT_ID('Banners'))
BEGIN
    CREATE INDEX IX_Banners_IsDeleted ON Banners(IsDeleted);
    PRINT '✓ Índice IX_Banners_IsDeleted creado';
END

-- Crear índice compuesto para consultas de banners activos no eliminados
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Activo_IsDeleted' AND object_id = OBJECT_ID('Banners'))
BEGIN
    CREATE INDEX IX_Banners_Activo_IsDeleted ON Banners(Activo, IsDeleted, Orden);
    PRINT '✓ Índice IX_Banners_Activo_IsDeleted creado';
END

PRINT '';
PRINT '=== SOFT DELETE IMPLEMENTADO ===';
PRINT 'Los banners ahora se marcan como eliminados en lugar de borrarse físicamente.';
PRINT 'Las consultas públicas automáticamente excluyen banners eliminados.';
GO

