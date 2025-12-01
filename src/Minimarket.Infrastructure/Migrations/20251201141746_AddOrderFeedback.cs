using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderFeedbacks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WebOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WouldRecommend = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderFeedbacks_WebOrders_WebOrderId",
                        column: x => x.WebOrderId,
                        principalTable: "WebOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderFeedbacks_WebOrderId",
                table: "OrderFeedbacks",
                column: "WebOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderFeedbacks");
        }
    }
}
