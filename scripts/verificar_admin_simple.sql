-- Script simple para verificar usuarios y roles
USE MinimarketDB;
GO

-- 1. Listar tablas relacionadas con User y Role
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME LIKE '%User%' OR TABLE_NAME LIKE '%Role%';
GO

-- 2. Ver primeros 5 usuarios
SELECT TOP 5 
    Id,
    UserName,
    Email,
    EmailConfirmed,
    LockoutEnabled,
    CASE 
        WHEN LockoutEnd IS NOT NULL AND LockoutEnd > GETUTCDATE() THEN 'Bloqueado'
        ELSE 'Activo'
    END AS Estado
FROM Users;
GO

-- 3. Ver todos los roles
SELECT 
    Id,
    Name,
    NormalizedName
FROM Roles;
GO

-- 4. Ver usuarios con sus roles
SELECT TOP 10
    u.UserName,
    u.Email,
    u.EmailConfirmed,
    r.Name AS Rol
FROM Users u
LEFT JOIN UserRoles ur ON u.Id = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.Id;
GO

-- 5. Verificar si existe admin con rol Administrador
SELECT 
    u.Id,
    u.UserName,
    u.Email,
    u.EmailConfirmed,
    u.LockoutEnabled,
    r.Name AS Rol,
    CASE 
        WHEN u.LockoutEnd IS NOT NULL AND u.LockoutEnd > GETUTCDATE() THEN 'Bloqueado'
        ELSE 'Activo'
    END AS Estado
FROM Users u
INNER JOIN UserRoles ur ON u.Id = ur.UserId
INNER JOIN Roles r ON ur.RoleId = r.Id
WHERE (u.UserName = 'admin' OR u.Email = 'admin@minimarketcamucha.com')
  AND (r.Name = 'Administrador' OR r.NormalizedName = 'ADMINISTRADOR');
GO

