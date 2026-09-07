using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnotadorGymAppApi.Migrations
{
    /// <inheritdoc />
    public partial class EntrenamientoRutinaDia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RutinaDiaId",
                table: "Entrenamientos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Entrenamientos_RutinaDiaId",
                table: "Entrenamientos",
                column: "RutinaDiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entrenamientos_RutinaDias_RutinaDiaId",
                table: "Entrenamientos",
                column: "RutinaDiaId",
                principalTable: "RutinaDias",
                principalColumn: "RutinaDiaId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entrenamientos_RutinaDias_RutinaDiaId",
                table: "Entrenamientos");

            migrationBuilder.DropIndex(
                name: "IX_Entrenamientos_RutinaDiaId",
                table: "Entrenamientos");

            migrationBuilder.DropColumn(
                name: "RutinaDiaId",
                table: "Entrenamientos");
        }
    }
}
