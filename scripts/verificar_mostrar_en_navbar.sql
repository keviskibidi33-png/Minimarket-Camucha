-- Script para verificar y corregir el campo MostrarEnNavbar en la base de datos
-- Ejecutar este script en SQL Server Management Studio

USE MinimarketDB;
GO

PRINT '=== VERIFICACIÓN Y CORRECCIÓN DE MostrarEnNavbar ===';
PRINT '';
GO

-- 1. Verificar que la columna existe
PRINT '1. Verificando columna MostrarEnNavbar...';
GO

IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('Pages') 
    AND name = 'MostrarEnNavbar'
)
BEGIN
    PRINT '   ✓ Columna MostrarEnNavbar existe';
END
ELSE
BEGIN
    PRINT '   ✗ ERROR: Columna MostrarEnNavbar NO existe';
    PRINT '   Ejecutar migración primero';
    RETURN;
END
GO

-- 2. Ver estado actual de las páginas
PRINT '';
PRINT '2. Estado actual de las páginas:';
GO

SELECT 
    Id,
    Titulo,
    Activa,
    MostrarEnNavbar,
    CASE 
        WHEN MostrarEnNavbar IS NULL THEN 'NULL'
        WHEN MostrarEnNavbar = 1 THEN 'true'
        ELSE 'false'
    END AS MostrarEnNavbarStatus
FROM Pages;
GO

-- 3. Si hay páginas activas sin MostrarEnNavbar definido, establecerlo
PRINT '';
PRINT '3. Corrigiendo valores NULL o estableciendo valores por defecto...';
GO

-- Establecer MostrarEnNavbar = false para páginas que tienen NULL
UPDATE Pages
SET MostrarEnNavbar = 0
WHERE MostrarEnNavbar IS NULL;
GO

-- 4. Mostrar resumen final
PRINT '';
PRINT '4. Resumen final:';
GO

SELECT 
    COUNT(*) AS TotalPaginas,
    SUM(CASE WHEN Activa = 1 THEN 1 ELSE 0 END) AS PaginasActivas,
    SUM(CASE WHEN MostrarEnNavbar = 1 THEN 1 ELSE 0 END) AS MostrarEnNavbarTrue,
    SUM(CASE WHEN Activa = 1 AND MostrarEnNavbar = 1 THEN 1 ELSE 0 END) AS ActivasYEnNavbar
FROM Pages;
GO

-- 5. Si quieres activar MostrarEnNavbar para todas las páginas activas (descomentar si necesario)
/*
PRINT '';
PRINT '5. Activando MostrarEnNavbar para todas las páginas activas...';
GO

UPDATE Pages
SET MostrarEnNavbar = 1
WHERE Activa = 1 AND MostrarEnNavbar = 0;
GO
*/

PRINT '';
PRINT '=== VERIFICACIÓN COMPLETA ===';
PRINT 'Si necesitas activar MostrarEnNavbar para páginas específicas, ejecuta:';
PRINT 'UPDATE Pages SET MostrarEnNavbar = 1 WHERE Id = ''<ID_DE_LA_PAGINA>'';';
GO

