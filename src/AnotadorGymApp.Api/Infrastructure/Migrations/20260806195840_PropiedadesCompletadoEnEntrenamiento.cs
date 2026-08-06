using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnotadorGymAppApi.Migrations
{
    /// <inheritdoc />
    public partial class PropiedadesCompletadoEnEntrenamiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Entrenamientos");

            migrationBuilder.AddColumn<bool>(
                name: "Completado",
                table: "Entrenamientos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Completado",
                table: "EjerciciosEntrenados",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Completado",
                table: "Entrenamientos");

            migrationBuilder.DropColumn(
                name: "Completado",
                table: "EjerciciosEntrenados");

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Entrenamientos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
