-- Script de migración para crear sedes iniciales si no existen
-- Este script verifica si existe una sede y la crea si no existe
-- También asegura que las rutas de imágenes sean correctas

USE MinimarketDB;
GO

-- Verificar si existe alguna sede
IF NOT EXISTS (SELECT 1 FROM Sedes)
BEGIN
    PRINT 'No hay sedes en la base de datos. Creando sede inicial...';
    
    -- Crear sede inicial (puedes ajustar los valores según necesites)
    DECLARE @SedeId UNIQUEIDENTIFIER = NEWID();
    DECLARE @HorariosJson NVARCHAR(2000) = '{"lunes":{"abre":"08:00","cierra":"18:00"},"martes":{"abre":"08:00","cierra":"18:00"},"miercoles":{"abre":"08:00","cierra":"18:00"},"jueves":{"abre":"08:00","cierra":"18:00"},"viernes":{"abre":"08:00","cierra":"18:00"},"sabado":{"abre":"08:00","cierra":"18:00"},"domingo":{"abre":"09:00","cierra":"14:00"}}';
    
    INSERT INTO Sedes (Id, Nombre, Direccion, Ciudad, Pais, Latitud, Longitud, Telefono, HorariosJson, LogoUrl, Estado, CreatedAt, UpdatedAt)
    VALUES (
        @SedeId,
        'Sede Principal',
        'Dirección principal del minimarket',
        'Ciudad',
        'Perú',
        0.0,  -- Latitud (ajustar según ubicación real)
        0.0,  -- Longitud (ajustar según ubicación real)
        NULL,
        @HorariosJson,
        NULL, -- LogoUrl inicialmente null, se puede actualizar después
        1,    -- Estado activo
        GETUTCDATE(),
        NULL
    );
    
    PRINT 'Sede inicial creada con ID: ' + CAST(@SedeId AS NVARCHAR(36));
END
ELSE
BEGIN
    PRINT 'Ya existen sedes en la base de datos.';
END
GO

-- Actualizar rutas de imágenes que usen http:// a https://
UPDATE Sedes
SET LogoUrl = REPLACE(LogoUrl, 'http://minimarket.edvio.app', 'https://minimarket.edvio.app')
WHERE LogoUrl LIKE 'http://minimarket.edvio.app%';
GO

UPDATE Sedes
SET LogoUrl = REPLACE(LogoUrl, 'http://api-minimarket.edvio.app', 'https://api-minimarket.edvio.app')
WHERE LogoUrl LIKE 'http://api-minimarket.edvio.app%';
GO

-- Verificar y actualizar rutas de angelqr.png si existe
-- Si hay una sede que referencia angelqr.png, asegurar que la ruta sea correcta
UPDATE Sedes
SET LogoUrl = 'uploads/sedes/angelqr.png'
WHERE LogoUrl LIKE '%angelqr%' 
  AND LogoUrl NOT LIKE 'uploads/%'
  AND LogoUrl NOT LIKE '/uploads/%';
GO

-- Mostrar todas las sedes y sus logos
SELECT 
    Id,
    Nombre,
    Direccion,
    Ciudad,
    LogoUrl,
    Estado,
    CreatedAt
FROM Sedes
ORDER BY CreatedAt;
GO

PRINT 'Migración de sedes completada.';
GO

