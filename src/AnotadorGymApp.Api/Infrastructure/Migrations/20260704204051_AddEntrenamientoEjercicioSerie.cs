using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnotadorGymAppApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEntrenamientoEjercicioSerie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entrenamiento",
                columns: table => new
                {
                    EntrenamientoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DuracionSegundos = table.Column<int>(type: "int", nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entrenamiento", x => x.EntrenamientoId);
                    table.ForeignKey(
                        name: "FK_Entrenamiento_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EjercicioEntrenado",
                columns: table => new
                {
                    EjercicioEntrenadoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntrenamientoId = table.Column<int>(type: "int", nullable: false),
                    EjercicioId = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EjercicioEntrenado", x => x.EjercicioEntrenadoId);
                    table.ForeignKey(
                        name: "FK_EjercicioEntrenado_Ejercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "EjercicioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EjercicioEntrenado_Entrenamiento_EntrenamientoId",
                        column: x => x.EntrenamientoId,
                        principalTable: "Entrenamiento",
                        principalColumn: "EntrenamientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SerieEntrenada",
                columns: table => new
                {
                    SerieEntrenadaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EjercicioEntrenadoId = table.Column<int>(type: "int", nullable: false),
                    NumeroSerie = table.Column<int>(type: "int", nullable: false),
                    Peso = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Repeticiones = table.Column<int>(type: "int", nullable: false),
                    Completada = table.Column<bool>(type: "bit", nullable: false),
                    FuePR = table.Column<bool>(type: "bit", nullable: false),
                    RPE = table.Column<int>(type: "int", nullable: true),
                    DescansoSegundos = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SerieEntrenada", x => x.SerieEntrenadaId);
                    table.ForeignKey(
                        name: "FK_SerieEntrenada_EjercicioEntrenado_EjercicioEntrenadoId",
                        column: x => x.EjercicioEntrenadoId,
                        principalTable: "EjercicioEntrenado",
                        principalColumn: "EjercicioEntrenadoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EjercicioEntrenado_EjercicioId",
                table: "EjercicioEntrenado",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_EjercicioEntrenado_EntrenamientoId",
                table: "EjercicioEntrenado",
                column: "EntrenamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_Entrenamiento_UsuarioId_Fecha",
                table: "Entrenamiento",
                columns: new[] { "UsuarioId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_SerieEntrenada_EjercicioEntrenadoId",
                table: "SerieEntrenada",
                column: "EjercicioEntrenadoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SerieEntrenada");

            migrationBuilder.DropTable(
                name: "EjercicioEntrenado");

            migrationBuilder.DropTable(
                name: "Entrenamiento");
        }
    }
}
