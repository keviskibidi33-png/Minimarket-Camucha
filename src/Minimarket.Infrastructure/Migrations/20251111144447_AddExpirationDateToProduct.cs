using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpirationDateToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "Products",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Products");
        }
    }
}
