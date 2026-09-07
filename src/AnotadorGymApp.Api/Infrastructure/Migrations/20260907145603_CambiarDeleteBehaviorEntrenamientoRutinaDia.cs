using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnotadorGymAppApi.Migrations
{
    /// <inheritdoc />
    public partial class CambiarDeleteBehaviorEntrenamientoRutinaDia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entrenamientos_RutinaDias_RutinaDiaId",
                table: "Entrenamientos");

            migrationBuilder.AddForeignKey(
                name: "FK_Entrenamientos_RutinaDias_RutinaDiaId",
                table: "Entrenamientos",
                column: "RutinaDiaId",
                principalTable: "RutinaDias",
                principalColumn: "RutinaDiaId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entrenamientos_RutinaDias_RutinaDiaId",
                table: "Entrenamientos");

            migrationBuilder.AddForeignKey(
                name: "FK_Entrenamientos_RutinaDias_RutinaDiaId",
                table: "Entrenamientos",
                column: "RutinaDiaId",
                principalTable: "RutinaDias",
                principalColumn: "RutinaDiaId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
