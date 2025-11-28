-- Script para asegurar que todos los banners existentes tengan IsDeleted = false
-- Esto garantiza que los banners existentes se muestren en el módulo de administración
USE MinimarketDB;
GO

SET NOCOUNT ON;
PRINT '=== CORRIGIENDO BANNERS EXISTENTES PARA SOFT DELETE ===';
PRINT '';

-- Verificar si la tabla existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Banners')
BEGIN
    PRINT 'ERROR: La tabla Banners no existe.';
    RETURN;
END

-- Verificar si la columna IsDeleted existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Banners') AND name = 'IsDeleted')
BEGIN
    PRINT 'ERROR: La columna IsDeleted no existe. Ejecuta primero add_soft_delete_to_banners.sql';
    RETURN;
END

-- Contar banners antes de la corrección
DECLARE @BannersAntes INT;
SELECT @BannersAntes = COUNT(*) FROM Banners;
PRINT 'Total de banners en la tabla: ' + CAST(@BannersAntes AS VARCHAR(10));

-- Contar banners con IsDeleted NULL o diferente de 0
DECLARE @BannersConProblema INT;
SELECT @BannersConProblema = COUNT(*) 
FROM Banners 
WHERE IsDeleted IS NULL OR IsDeleted != 0;
PRINT 'Banners que necesitan corrección: ' + CAST(@BannersConProblema AS VARCHAR(10));

-- Corregir banners existentes: establecer IsDeleted = 0 si es NULL o diferente de 0
-- Esto asegura que todos los banners existentes se muestren en el backoffice
UPDATE Banners
SET IsDeleted = 0,
    DeletedAt = NULL
WHERE IsDeleted IS NULL OR IsDeleted != 0;

DECLARE @BannersCorregidos INT = @@ROWCOUNT;
PRINT 'Banners corregidos: ' + CAST(@BannersCorregidos AS VARCHAR(10));

-- Verificar estado final
DECLARE @BannersActivos INT;
DECLARE @BannersInactivos INT;
DECLARE @BannersEliminados INT;

SELECT @BannersActivos = COUNT(*) FROM Banners WHERE Activo = 1 AND IsDeleted = 0;
SELECT @BannersInactivos = COUNT(*) FROM Banners WHERE Activo = 0 AND IsDeleted = 0;
SELECT @BannersEliminados = COUNT(*) FROM Banners WHERE IsDeleted = 1;

PRINT '';
PRINT '=== ESTADO FINAL DE BANNERS ===';
PRINT 'Total de banners: ' + CAST(@BannersAntes AS VARCHAR(10));
PRINT 'Banners activos (no eliminados): ' + CAST(@BannersActivos AS VARCHAR(10));
PRINT 'Banners inactivos (no eliminados): ' + CAST(@BannersInactivos AS VARCHAR(10));
PRINT 'Banners eliminados (soft delete): ' + CAST(@BannersEliminados AS VARCHAR(10));
PRINT '';
PRINT '✓ Todos los banners existentes ahora tienen IsDeleted = 0';
PRINT '✓ Los banners se mostrarán correctamente en el módulo de administración';
GO

