-- Script para activar MostrarEnNavbar en una página específica
-- Reemplaza <ID_DE_LA_PAGINA> con el ID real de la página que quieres activar

USE MinimarketDB;
GO

-- Opción 1: Activar MostrarEnNavbar para TODAS las páginas activas
PRINT 'Activando MostrarEnNavbar para todas las páginas activas...';
GO

UPDATE Pages
SET MostrarEnNavbar = 1
WHERE Activa = 1;
GO

PRINT 'Páginas actualizadas:';
SELECT 
    Id,
    Titulo,
    Activa,
    MostrarEnNavbar
FROM Pages
WHERE Activa = 1;
GO

PRINT '=== COMPLETADO ===';
PRINT 'Todas las páginas activas ahora tienen MostrarEnNavbar = 1';
GO

-- Opción 2: Si solo quieres activar una página específica, descomenta y usa esto:
/*
UPDATE Pages
SET MostrarEnNavbar = 1
WHERE Id = 'TU-ID-AQUI';
GO
*/

