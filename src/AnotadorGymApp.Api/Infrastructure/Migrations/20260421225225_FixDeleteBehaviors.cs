using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnotadorGymAppApi.Migrations
{
    /// <inheritdoc />
    public partial class FixDeleteBehaviors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EjercicioMusculoSecundarios_Ejercicios_EjerciciosSecundariosEjercicioId",
                table: "EjercicioMusculoSecundarios");

            migrationBuilder.DropForeignKey(
                name: "FK_EjercicioMusculoSecundarios_Musculos_MusculosSecundariosMusculoId",
                table: "EjercicioMusculoSecundarios");

            migrationBuilder.RenameColumn(
                name: "MusculosSecundariosMusculoId",
                table: "EjercicioMusculoSecundarios",
                newName: "MusculoId");

            migrationBuilder.RenameColumn(
                name: "EjerciciosSecundariosEjercicioId",
                table: "EjercicioMusculoSecundarios",
                newName: "EjercicioId");

            migrationBuilder.RenameIndex(
                name: "IX_EjercicioMusculoSecundarios_MusculosSecundariosMusculoId",
                table: "EjercicioMusculoSecundarios",
                newName: "IX_EjercicioMusculoSecundarios_MusculoId");

            migrationBuilder.AddForeignKey(
                name: "FK_EjercicioMusculoSecundarios_Ejercicios_EjercicioId",
                table: "EjercicioMusculoSecundarios",
                column: "EjercicioId",
                principalTable: "Ejercicios",
                principalColumn: "EjercicioId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EjercicioMusculoSecundarios_Musculos_MusculoId",
                table: "EjercicioMusculoSecundarios",
                column: "MusculoId",
                principalTable: "Musculos",
                principalColumn: "MusculoId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EjercicioMusculoSecundarios_Ejercicios_EjercicioId",
                table: "EjercicioMusculoSecundarios");

            migrationBuilder.DropForeignKey(
                name: "FK_EjercicioMusculoSecundarios_Musculos_MusculoId",
                table: "EjercicioMusculoSecundarios");

            migrationBuilder.RenameColumn(
                name: "MusculoId",
                table: "EjercicioMusculoSecundarios",
                newName: "MusculosSecundariosMusculoId");

            migrationBuilder.RenameColumn(
                name: "EjercicioId",
                table: "EjercicioMusculoSecundarios",
                newName: "EjerciciosSecundariosEjercicioId");

            migrationBuilder.RenameIndex(
                name: "IX_EjercicioMusculoSecundarios_MusculoId",
                table: "EjercicioMusculoSecundarios",
                newName: "IX_EjercicioMusculoSecundarios_MusculosSecundariosMusculoId");

            migrationBuilder.AddForeignKey(
                name: "FK_EjercicioMusculoSecundarios_Ejercicios_EjerciciosSecundariosEjercicioId",
                table: "EjercicioMusculoSecundarios",
                column: "EjerciciosSecundariosEjercicioId",
                principalTable: "Ejercicios",
                principalColumn: "EjercicioId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EjercicioMusculoSecundarios_Musculos_MusculosSecundariosMusculoId",
                table: "EjercicioMusculoSecundarios",
                column: "MusculosSecundariosMusculoId",
                principalTable: "Musculos",
                principalColumn: "MusculoId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
