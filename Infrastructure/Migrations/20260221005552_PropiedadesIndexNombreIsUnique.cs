using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnotadorGymAppApi.Migrations
{
    /// <inheritdoc />
    public partial class PropiedadesIndexNombreIsUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Rutinas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Ejercicios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Rutinas_Nombre",
                table: "Rutinas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_Nombre",
                table: "Ejercicios",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rutinas_Nombre",
                table: "Rutinas");

            migrationBuilder.DropIndex(
                name: "IX_Ejercicios_Nombre",
                table: "Ejercicios");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Rutinas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Ejercicios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
