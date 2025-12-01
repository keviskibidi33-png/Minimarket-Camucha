using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBannersTableStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eliminar índices solo si existen
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_IsActive' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    DROP INDEX IX_Banners_IsActive ON Banners;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Position' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    DROP INDEX IX_Banners_Position ON Banners;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_DisplayOrder' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    DROP INDEX IX_Banners_DisplayOrder ON Banners;
                END
            ");

            // Eliminar columnas solo si existen
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE name = 'IsActive' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners DROP COLUMN IsActive;
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE name = 'Position' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners DROP COLUMN Position;
                END
            ");

            // Renombrar columnas solo si existen
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE name = 'Title' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    EXEC sp_rename 'Banners.Title', 'Titulo', 'COLUMN';
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE name = 'StartDate' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    EXEC sp_rename 'Banners.StartDate', 'FechaInicio', 'COLUMN';
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE name = 'LinkUrl' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    EXEC sp_rename 'Banners.LinkUrl', 'UrlDestino', 'COLUMN';
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE name = 'ImageUrl' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    EXEC sp_rename 'Banners.ImageUrl', 'ImagenUrl', 'COLUMN';
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE name = 'EndDate' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    EXEC sp_rename 'Banners.EndDate', 'FechaFin', 'COLUMN';
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE name = 'DisplayOrder' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    EXEC sp_rename 'Banners.DisplayOrder', 'Orden', 'COLUMN';
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.columns WHERE name = 'Description' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    EXEC sp_rename 'Banners.Description', 'Descripcion', 'COLUMN';
                END
            ");

            // Agregar columna Tipo si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'Tipo' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD Tipo INT NOT NULL DEFAULT 0;
                END
            ");

            // Agregar columna ClasesCss si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'ClasesCss' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD ClasesCss NVARCHAR(500) NULL;
                END
            ");

            // Agregar columnas solo si no existen
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'AbrirEnNuevaVentana' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD AbrirEnNuevaVentana BIT NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'Activo' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD Activo BIT NOT NULL DEFAULT 1;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'AltoMaximo' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD AltoMaximo INT NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'AnchoMaximo' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD AnchoMaximo INT NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'DeletedAt' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD DeletedAt DATETIME2 NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'Descripcion' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD Descripcion NVARCHAR(1000) NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'IsDeleted' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD IsDeleted BIT NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'MaxVisualizaciones' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD MaxVisualizaciones INT NULL;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'Orden' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD Orden INT NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'Posicion' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD Posicion INT NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'SoloDesktop' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD SoloDesktop BIT NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'SoloMovil' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD SoloMovil BIT NOT NULL DEFAULT 0;
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'VisualizacionesActuales' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    ALTER TABLE Banners ADD VisualizacionesActuales INT NOT NULL DEFAULT 0;
                END
            ");

            // Crear índices solo si no existen
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Activo_FechaInicio_FechaFin' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    CREATE INDEX IX_Banners_Activo_FechaInicio_FechaFin ON Banners(Activo, FechaInicio, FechaFin);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Orden' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    CREATE INDEX IX_Banners_Orden ON Banners(Orden);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Posicion' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    CREATE INDEX IX_Banners_Posicion ON Banners(Posicion);
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Tipo' AND object_id = OBJECT_ID('Banners'))
                BEGIN
                    CREATE INDEX IX_Banners_Tipo ON Banners(Tipo);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Banners_Activo_FechaInicio_FechaFin",
                table: "Banners");

            migrationBuilder.DropIndex(
                name: "IX_Banners_Orden",
                table: "Banners");

            migrationBuilder.DropIndex(
                name: "IX_Banners_Posicion",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "AbrirEnNuevaVentana",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "AltoMaximo",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "AnchoMaximo",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "MaxVisualizaciones",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "Orden",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "Posicion",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "SoloDesktop",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "SoloMovil",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "VisualizacionesActuales",
                table: "Banners");

            migrationBuilder.RenameColumn(
                name: "UrlDestino",
                table: "Banners",
                newName: "LinkUrl");

            migrationBuilder.RenameColumn(
                name: "Titulo",
                table: "Banners",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "Banners",
                newName: "DisplayOrder");

            migrationBuilder.RenameColumn(
                name: "ImagenUrl",
                table: "Banners",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "FechaInicio",
                table: "Banners",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "FechaFin",
                table: "Banners",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "ClasesCss",
                table: "Banners",
                newName: "Description");

            migrationBuilder.RenameIndex(
                name: "IX_Banners_Tipo",
                table: "Banners",
                newName: "IX_Banners_DisplayOrder");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Banners",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Position",
                table: "Banners",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Banners_IsActive",
                table: "Banners",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Banners_Position",
                table: "Banners",
                column: "Position");
        }
    }
}
