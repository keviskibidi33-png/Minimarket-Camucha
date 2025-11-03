-- scripts/01_CreateDatabase.sql
-- Script para crear la base de datos MinimarketDB

USE master;
GO

-- Drop database if exists (solo para desarrollo)
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'MinimarketDB')
BEGIN
    ALTER DATABASE MinimarketDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MinimarketDB;
END
GO

-- Crear base de datos
CREATE DATABASE MinimarketDB
ON PRIMARY
(
    NAME = N'MinimarketDB_Data',
    FILENAME = N'C:\SQLData\MinimarketDB.mdf',
    SIZE = 100MB,
    MAXSIZE = UNLIMITED,
    FILEGROWTH = 10MB
)
LOG ON
(
    NAME = N'MinimarketDB_Log',
    FILENAME = N'C:\SQLData\MinimarketDB_log.ldf',
    SIZE = 50MB,
    MAXSIZE = 1GB,
    FILEGROWTH = 10MB
);
GO

-- Configurar opciones de base de datos
ALTER DATABASE MinimarketDB SET RECOVERY SIMPLE;
ALTER DATABASE MinimarketDB SET READ_COMMITTED_SNAPSHOT ON;
GO

USE MinimarketDB;
GO

PRINT 'Database MinimarketDB created successfully!';
GO

