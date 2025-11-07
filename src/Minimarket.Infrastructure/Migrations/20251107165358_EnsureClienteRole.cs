using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Minimarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnsureClienteRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear el rol "Cliente" si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Cliente')
                BEGIN
                    INSERT INTO Roles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES (NEWID(), 'Cliente', 'CLIENTE', NEWID())
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
