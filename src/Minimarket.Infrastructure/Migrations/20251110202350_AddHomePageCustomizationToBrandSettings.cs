using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHomePageCustomizationToBrandSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HomeBannerImageUrl",
                table: "BrandSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeDescription",
                table: "BrandSettings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeSubtitle",
                table: "BrandSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomeTitle",
                table: "BrandSettings",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomeBannerImageUrl",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "HomeDescription",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "HomeSubtitle",
                table: "BrandSettings");

            migrationBuilder.DropColumn(
                name: "HomeTitle",
                table: "BrandSettings");
        }
    }
}
