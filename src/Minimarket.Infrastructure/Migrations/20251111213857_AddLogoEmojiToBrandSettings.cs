using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLogoEmojiToBrandSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoEmoji",
                table: "BrandSettings",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoEmoji",
                table: "BrandSettings");
        }
    }
}
