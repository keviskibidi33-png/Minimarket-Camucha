using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppPhoneToBrandSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentProofUrl",
                table: "WebOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppPhone",
                table: "BrandSettings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentProofUrl",
                table: "WebOrders");

            migrationBuilder.DropColumn(
                name: "WhatsAppPhone",
                table: "BrandSettings");
        }
    }
}
