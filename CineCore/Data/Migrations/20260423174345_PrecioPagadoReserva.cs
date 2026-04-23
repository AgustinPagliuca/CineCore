using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CineCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class PrecioPagadoReserva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioPagado",
                table: "Reservas",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecioPagado",
                table: "Reservas");
        }
    }
}
