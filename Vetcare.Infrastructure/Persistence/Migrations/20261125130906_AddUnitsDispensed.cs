using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetcare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitsDispensed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnidadesEntregadas",
                table: "Prescripciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnidadesEntregadas",
                table: "Prescripciones");
        }
    }
}
