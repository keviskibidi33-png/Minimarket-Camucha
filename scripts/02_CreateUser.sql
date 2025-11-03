-- scripts/02_CreateUser.sql
-- Script para crear usuario y permisos

USE master;
GO

-- Crear login
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'minimarket_app')
BEGIN
    CREATE LOGIN minimarket_app WITH PASSWORD = 'Minimarket@2024!';
END
GO

USE MinimarketDB;
GO

-- Crear usuario en la base de datos
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'minimarket_app')
BEGIN
    CREATE USER minimarket_app FOR LOGIN minimarket_app;
END
GO

-- Asignar permisos
ALTER ROLE db_datareader ADD MEMBER minimarket_app;
ALTER ROLE db_datawriter ADD MEMBER minimarket_app;
GRANT EXECUTE TO minimarket_app;
GRANT VIEW DEFINITION TO minimarket_app;
GO

PRINT 'User minimarket_app created with appropriate permissions!';
GO

