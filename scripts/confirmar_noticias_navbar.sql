-- Script de confirmación: Verificar que las noticias están configuradas para el navbar
USE MinimarketDB;
GO

PRINT '=== CONFIRMACIÓN: NOTICIAS EN NAVBAR ===';
PRINT '';
GO

-- Ver todas las páginas y su estado
PRINT 'Estado de todas las páginas:';
GO

SELECT 
    Id,
    Titulo,
    Activa,
    MostrarEnNavbar,
    CASE 
        WHEN Activa = 1 AND MostrarEnNavbar = 1 THEN '✓ Visible en Navbar'
        WHEN Activa = 1 AND MostrarEnNavbar = 0 THEN '⚠ Activa pero NO en Navbar'
        WHEN Activa = 0 THEN '✗ Inactiva'
        ELSE '? Estado desconocido'
    END AS EstadoNavbar
FROM Pages
ORDER BY Activa DESC, MostrarEnNavbar DESC;
GO

-- Resumen final
PRINT '';
PRINT 'Resumen:';
GO

SELECT 
    COUNT(*) AS TotalPaginas,
    SUM(CASE WHEN Activa = 1 THEN 1 ELSE 0 END) AS PaginasActivas,
    SUM(CASE WHEN MostrarEnNavbar = 1 THEN 1 ELSE 0 END) AS MostrarEnNavbarTrue,
    SUM(CASE WHEN Activa = 1 AND MostrarEnNavbar = 1 THEN 1 ELSE 0 END) AS ActivasYEnNavbar,
    SUM(CASE WHEN Activa = 1 AND MostrarEnNavbar = 0 THEN 1 ELSE 0 END) AS ActivasPeroNoEnNavbar
FROM Pages;
GO

-- Verificar setting global
PRINT '';
PRINT 'Configuración global enable_news_in_navbar:';
GO

SELECT 
    [Key],
    Value,
    IsActive,
    CASE 
        WHEN Value = 'true' OR Value = '1' THEN '✓ Activado'
        ELSE '✗ Desactivado'
    END AS Estado
FROM SystemSettings
WHERE [Key] = 'enable_news_in_navbar';
GO

PRINT '';
PRINT '=== CONFIRMACIÓN COMPLETA ===';
PRINT 'Si ActivasYEnNavbar > 0, las noticias deberían aparecer en el navbar del frontend.';
PRINT 'Recarga la página del frontend y verifica en la consola del navegador.';
GO
