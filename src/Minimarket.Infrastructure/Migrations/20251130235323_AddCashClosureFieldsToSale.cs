using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCashClosureFieldsToSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Banners");

            migrationBuilder.AddColumn<DateTime>(
                name: "CashClosureDate",
                table: "Sales",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "Sales",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Sales_IsClosed",
                table: "Sales",
                column: "IsClosed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sales_IsClosed",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "CashClosureDate",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "Sales");

            migrationBuilder.CreateTable(
                name: "Banners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AbrirEnNuevaVentana = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AltoMaximo = table.Column<int>(type: "int", nullable: true),
                    AnchoMaximo = table.Column<int>(type: "int", nullable: true),
                    ClasesCss = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImagenUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    MaxVisualizaciones = table.Column<int>(type: "int", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Posicion = table.Column<int>(type: "int", nullable: false),
                    SoloDesktop = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SoloMovil = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UrlDestino = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VisualizacionesActuales = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banners", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Banners_Activo_FechaInicio_FechaFin",
                table: "Banners",
                columns: new[] { "Activo", "FechaInicio", "FechaFin" });

            migrationBuilder.CreateIndex(
                name: "IX_Banners_Orden",
                table: "Banners",
                column: "Orden");

            migrationBuilder.CreateIndex(
                name: "IX_Banners_Posicion",
                table: "Banners",
                column: "Posicion");

            migrationBuilder.CreateIndex(
                name: "IX_Banners_Tipo",
                table: "Banners",
                column: "Tipo");
        }
    }
}
