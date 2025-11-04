-- scripts/01_CreateDatabase_Simple.sql
-- Script simplificado para crear la base de datos MinimarketDB
-- Usa la ruta predeterminada de SQL Server (sin especificar rutas personalizadas)

USE master;
GO

-- Drop database if exists (solo para desarrollo)
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'MinimarketDB')
BEGIN
    ALTER DATABASE MinimarketDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MinimarketDB;
END
GO

-- Crear base de datos (SQL Server usará su ruta predeterminada automáticamente)
CREATE DATABASE MinimarketDB;
GO

-- Configurar opciones de base de datos
USE MinimarketDB;
GO

ALTER DATABASE MinimarketDB SET RECOVERY SIMPLE;
ALTER DATABASE MinimarketDB SET READ_COMMITTED_SNAPSHOT ON;
GO

PRINT 'Database MinimarketDB created successfully!';
GO

