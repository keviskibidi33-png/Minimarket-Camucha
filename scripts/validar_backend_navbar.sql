-- Script de validación para verificar que el backend maneja correctamente los cambios de navbar
-- Ejecutar este script en SQL Server Management Studio

USE MinimarketDB;
GO

PRINT '=== VALIDACIÓN DE BACKEND PARA NAVBAR DE NOTICIAS ===';
PRINT '';

-- 1. Verificar que existe la columna MostrarEnNavbar en Pages
PRINT '1. Verificando columna MostrarEnNavbar en tabla Pages...';
IF EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('Pages') 
    AND name = 'MostrarEnNavbar'
)
BEGIN
    PRINT '   ✓ Columna MostrarEnNavbar existe en Pages';
END
ELSE
BEGIN
    PRINT '   ✗ ERROR: Columna MostrarEnNavbar NO existe en Pages';
    PRINT '   Ejecutar: scripts/apply_mostrar_en_navbar_migration.sql';
END
GO

-- 2. Verificar que existe el setting enable_news_in_navbar
PRINT '';
PRINT '2. Verificando setting enable_news_in_navbar...';
IF EXISTS (
    SELECT 1 
    FROM SystemSettings 
    WHERE Key = 'enable_news_in_navbar'
)
BEGIN
    DECLARE @enableValue NVARCHAR(100);
    SELECT @enableValue = Value FROM SystemSettings WHERE Key = 'enable_news_in_navbar';
    PRINT '   ✓ Setting enable_news_in_navbar existe';
    PRINT '   Valor actual: ' + @enableValue;
END
ELSE
BEGIN
    PRINT '   ⚠ Setting enable_news_in_navbar NO existe (se creará automáticamente al guardar desde el frontend)';
    PRINT '   O ejecutar manualmente:';
    PRINT '   INSERT INTO SystemSettings (Id, Key, Value, Description, Category, IsActive, CreatedAt, UpdatedAt)';
    PRINT '   VALUES (NEWID(), ''enable_news_in_navbar'', ''true'', ''Activar o desactivar la funcionalidad de mostrar noticias en el navbar'', ''navbar'', 1, GETUTCDATE(), GETUTCDATE());';
END
GO

-- 3. Verificar que hay al menos una noticia activa
PRINT '';
PRINT '3. Verificando noticias activas...';
DECLARE @activePagesCount INT;
SELECT @activePagesCount = COUNT(*) FROM Pages WHERE Activa = 1;
IF @activePagesCount > 0
BEGIN
    PRINT '   ✓ Hay ' + CAST(@activePagesCount AS NVARCHAR(10)) + ' noticia(s) activa(s)';
END
ELSE
BEGIN
    PRINT '   ⚠ ADVERTENCIA: No hay noticias activas. Debe haber al menos una.';
END
GO

-- 4. Verificar noticias con mostrarEnNavbar = true
PRINT '';
PRINT '4. Verificando noticias configuradas para navbar...';
DECLARE @navbarPagesCount INT;
SELECT @navbarPagesCount = COUNT(*) FROM Pages WHERE MostrarEnNavbar = 1 AND Activa = 1;
PRINT '   Noticias activas y visibles en navbar: ' + CAST(@navbarPagesCount AS NVARCHAR(10));
GO

-- 5. Mostrar resumen de configuración
PRINT '';
PRINT '=== RESUMEN DE CONFIGURACIÓN ===';
SELECT 
    'Setting Global' AS Tipo,
    Key AS Configuracion,
    Value AS Valor,
    IsActive AS Activo
FROM SystemSettings 
WHERE Key = 'enable_news_in_navbar'
UNION ALL
SELECT 
    'Noticia' AS Tipo,
    Titulo AS Configuracion,
    CASE 
        WHEN Activa = 1 AND MostrarEnNavbar = 1 THEN 'Activa + Navbar'
        WHEN Activa = 1 THEN 'Activa'
        WHEN MostrarEnNavbar = 1 THEN 'Navbar (inactiva)'
        ELSE 'Inactiva'
    END AS Valor,
    CAST(Activa AS BIT) AS Activo
FROM Pages
ORDER BY Tipo, Configuracion;
GO

PRINT '';
PRINT '=== VALIDACIÓN COMPLETA ===';
PRINT 'Si todos los checks muestran ✓, el backend está configurado correctamente.';
GO

