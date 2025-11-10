using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentAndDeliverySettingsToBrandSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankAccountNumber",
                table: "BrandSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccountType",
                table: "BrandSettings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BankAccountVisible",
                table: "BrandSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BankCCI",
                table: "BrandSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "BrandSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryCost",
                table: "BrandSettings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryType",
                table: "BrandSettings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Ambos");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryZones",
                table: "BrandSettings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PlinEnabled",
                table: "BrandSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PlinPhone",
                table: "BrandSettings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlinQRUrl",
                table: "BrandSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "YapeEnabled",
                table: "BrandSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "YapePhone",
                table: "BrandSettings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YapeQRUrl",
                table: "BrandSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankAccountNumber",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "BankAccountType",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "BankAccountVisible",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "BankCCI",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "DeliveryCost",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "DeliveryType",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "DeliveryZones",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "PlinEnabled",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "PlinPhone",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "PlinQRUrl",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "YapeEnabled",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "YapePhone",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "YapeQRUrl",
                table: "BrandSettings");
        }
    }
}
