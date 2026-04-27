using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnotadorGymAppApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GrupoMusculares",
                columns: table => new
                {
                    GrupoMuscularId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrupoMusculares", x => x.GrupoMuscularId);
                });

            migrationBuilder.CreateTable(
                name: "Musculos",
                columns: table => new
                {
                    MusculoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Musculos", x => x.MusculoId);
                });

            migrationBuilder.CreateTable(
                name: "Rutinas",
                columns: table => new
                {
                    RutinaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TiempoPorSesion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dificultad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FrecuenciaPorGrupo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rutinas", x => x.RutinaId);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "invitado")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                });

            migrationBuilder.CreateTable(
                name: "Ejercicios",
                columns: table => new
                {
                    EjercicioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MusculoPrimarioId = table.Column<int>(type: "int", nullable: false),
                    GrupoMuscularId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    RutinaSemanaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroSemana = table.Column<int>(type: "int", nullable: false),
                    RutinaId = table.Column<int>(type: "int", nullable: false),
                    NombreSemana = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    EjerciciosSecundariosEjercicioId = table.Column<int>(type: "int", nullable: false),
                    MusculosSecundariosMusculoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EjercicioMusculoSecundarios", x => new { x.EjerciciosSecundariosEjercicioId, x.MusculosSecundariosMusculoId });
                    table.ForeignKey(
                        name: "FK_EjercicioMusculoSecundarios_Ejercicios_EjerciciosSecundariosEjercicioId",
                        column: x => x.EjerciciosSecundariosEjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "EjercicioId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EjercicioMusculoSecundarios_Musculos_MusculosSecundariosMusculoId",
                        column: x => x.MusculosSecundariosMusculoId,
                        principalTable: "Musculos",
                        principalColumn: "MusculoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RutinaDias",
                columns: table => new
                {
                    RutinaDiaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroDia = table.Column<int>(type: "int", nullable: false),
                    RutinaSemanaId = table.Column<int>(type: "int", nullable: false),
                    NombreRutinaDia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    RutinaEjercicioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroEjercicio = table.Column<int>(type: "int", nullable: false),
                    RutinaDiaId = table.Column<int>(type: "int", nullable: false),
                    EjercicioId = table.Column<int>(type: "int", nullable: false)
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
                    RutinaSerieId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroSerie = table.Column<int>(type: "int", nullable: false),
                    RutinaEjercicioId = table.Column<int>(type: "int", nullable: false),
                    Descanso = table.Column<TimeSpan>(type: "time", nullable: true),
                    Repeticiones = table.Column<int>(type: "int", nullable: true),
                    Porcentaje1RM = table.Column<int>(type: "int", nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false)
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
                name: "IX_Ejercicios_Nombre",
                table: "Ejercicios",
                column: "Nombre",
                unique: true);

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
                name: "IX_Rutinas_Nombre",
                table: "Rutinas",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RutinaSemanas_RutinaId",
                table: "RutinaSemanas",
                column: "RutinaId");

            migrationBuilder.CreateIndex(
                name: "IX_RutinaSeries_RutinaEjercicioId",
                table: "RutinaSeries",
                column: "RutinaEjercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_UserName",
                table: "Usuarios",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EjercicioMusculoSecundarios");

            migrationBuilder.DropTable(
                name: "RutinaSeries");

            migrationBuilder.DropTable(
                name: "Usuarios");

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
