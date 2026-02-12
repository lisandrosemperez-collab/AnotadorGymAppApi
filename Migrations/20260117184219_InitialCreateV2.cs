using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AnotadorGymAppApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrupoMusculares",
                columns: table => new
                {
                    GrupoMuscularId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoMusculares", x => x.GrupoMuscularId);
                });

            migrationBuilder.CreateTable(
                name: "Musculos",
                columns: table => new
                {
                    MusculoId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Musculos", x => x.MusculoId);
                });

            migrationBuilder.CreateTable(
                name: "Rutinas",
                columns: table => new
                {
                    RutinaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ImageSource = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    TiempoPorSesion = table.Column<string>(type: "text", nullable: false),
                    Dificultad = table.Column<string>(type: "text", nullable: false),
                    FrecuenciaPorGrupo = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rutinas", x => x.RutinaId);
                });

            migrationBuilder.CreateTable(
                name: "Ejercicios",
                columns: table => new
                {
                    EjercicioId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MusculoPrimarioId = table.Column<int>(type: "integer", nullable: false),
                    GrupoMuscularId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ejercicios", x => x.EjercicioId);
                    table.ForeignKey(
                        name: "FK_Ejercicios_GrupoMusculares_GrupoMuscularId",
                        column: x => x.GrupoMuscularId,
                        principalTable: "GrupoMusculares",
                        principalColumn: "GrupoMuscularId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ejercicios_Musculos_MusculoPrimarioId",
                        column: x => x.MusculoPrimarioId,
                        principalTable: "Musculos",
                        principalColumn: "MusculoId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RutinaSemanas",
                columns: table => new
                {
                    RutinaSemanaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RutinaId = table.Column<int>(type: "integer", nullable: false),
                    NombreSemana = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RutinaSemanas", x => x.RutinaSemanaId);
                    table.ForeignKey(
                        name: "FK_RutinaSemanas_Rutinas_RutinaId",
                        column: x => x.RutinaId,
                        principalTable: "Rutinas",
                        principalColumn: "RutinaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EjercicioMusculoSecundarios",
                columns: table => new
                {
                    EjerciciosSecundariosEjercicioId = table.Column<int>(type: "integer", nullable: false),
                    MusculosSecundariosMusculoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EjercicioMusculoSecundarios", x => new { x.EjerciciosSecundariosEjercicioId, x.MusculosSecundariosMusculoId });
                    table.ForeignKey(
                        name: "FK_EjercicioMusculoSecundarios_Ejercicios_EjerciciosSecundario~",
                        column: x => x.EjerciciosSecundariosEjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "EjercicioId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EjercicioMusculoSecundarios_Musculos_MusculosSecundariosMus~",
                        column: x => x.MusculosSecundariosMusculoId,
                        principalTable: "Musculos",
                        principalColumn: "MusculoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RutinaDias",
                columns: table => new
                {
                    RutinaDiaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RutinaSemanaId = table.Column<int>(type: "integer", nullable: false),
                    NombreRutinaDia = table.Column<string>(type: "text", nullable: false),
                    Notas = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RutinaDias", x => x.RutinaDiaId);
                    table.ForeignKey(
                        name: "FK_RutinaDias_RutinaSemanas_RutinaSemanaId",
                        column: x => x.RutinaSemanaId,
                        principalTable: "RutinaSemanas",
                        principalColumn: "RutinaSemanaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RutinaEjercicios",
                columns: table => new
                {
                    RutinaEjercicioId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RutinaDiaId = table.Column<int>(type: "integer", nullable: false),
                    EjercicioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RutinaEjercicios", x => x.RutinaEjercicioId);
                    table.ForeignKey(
                        name: "FK_RutinaEjercicios_Ejercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "EjercicioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RutinaEjercicios_RutinaDias_RutinaDiaId",
                        column: x => x.RutinaDiaId,
                        principalTable: "RutinaDias",
                        principalColumn: "RutinaDiaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RutinaSeries",
                columns: table => new
                {
                    RutinaSerieId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RutinaEjercicioId = table.Column<int>(type: "integer", nullable: false),
                    Descanso = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Repeticiones = table.Column<int>(type: "integer", nullable: true),
                    Porcentaje1RM = table.Column<int>(type: "integer", nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RutinaSeries", x => x.RutinaSerieId);
                    table.ForeignKey(
                        name: "FK_RutinaSeries_RutinaEjercicios_RutinaEjercicioId",
                        column: x => x.RutinaEjercicioId,
                        principalTable: "RutinaEjercicios",
                        principalColumn: "RutinaEjercicioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EjercicioMusculoSecundarios_MusculosSecundariosMusculoId",
                table: "EjercicioMusculoSecundarios",
                column: "MusculosSecundariosMusculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_GrupoMuscularId",
                table: "Ejercicios",
                column: "GrupoMuscularId");

            migrationBuilder.CreateIndex(
                name: "IX_Ejercicios_MusculoPrimarioId",
                table: "Ejercicios",
                column: "MusculoPrimarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RutinaDias_RutinaSemanaId",
                table: "RutinaDias",
                column: "RutinaSemanaId");

            migrationBuilder.CreateIndex(
                name: "IX_RutinaEjercicios_EjercicioId",
                table: "RutinaEjercicios",
                column: "EjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_RutinaEjercicios_RutinaDiaId",
                table: "RutinaEjercicios",
                column: "RutinaDiaId");

            migrationBuilder.CreateIndex(
                name: "IX_RutinaSemanas_RutinaId",
                table: "RutinaSemanas",
                column: "RutinaId");

            migrationBuilder.CreateIndex(
                name: "IX_RutinaSeries_RutinaEjercicioId",
                table: "RutinaSeries",
                column: "RutinaEjercicioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EjercicioMusculoSecundarios");

            migrationBuilder.DropTable(
                name: "RutinaSeries");

            migrationBuilder.DropTable(
                name: "RutinaEjercicios");

            migrationBuilder.DropTable(
                name: "Ejercicios");

            migrationBuilder.DropTable(
                name: "RutinaDias");

            migrationBuilder.DropTable(
                name: "GrupoMusculares");

            migrationBuilder.DropTable(
                name: "Musculos");

            migrationBuilder.DropTable(
                name: "RutinaSemanas");

            migrationBuilder.DropTable(
                name: "Rutinas");
        }
    }
}
