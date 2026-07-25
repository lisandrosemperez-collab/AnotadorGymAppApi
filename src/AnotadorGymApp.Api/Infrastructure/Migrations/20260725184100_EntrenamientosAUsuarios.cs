using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnotadorGymAppApi.Migrations
{
    /// <inheritdoc />
    public partial class EntrenamientosAUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EjercicioEntrenado_Ejercicios_EjercicioId",
                table: "EjercicioEntrenado");

            migrationBuilder.DropForeignKey(
                name: "FK_EjercicioEntrenado_Entrenamiento_EntrenamientoId",
                table: "EjercicioEntrenado");

            migrationBuilder.DropForeignKey(
                name: "FK_Entrenamiento_Usuarios_UsuarioId",
                table: "Entrenamiento");

            migrationBuilder.DropForeignKey(
                name: "FK_SerieEntrenada_EjercicioEntrenado_EjercicioEntrenadoId",
                table: "SerieEntrenada");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SerieEntrenada",
                table: "SerieEntrenada");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Entrenamiento",
                table: "Entrenamiento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EjercicioEntrenado",
                table: "EjercicioEntrenado");

            migrationBuilder.RenameTable(
                name: "SerieEntrenada",
                newName: "SeriesEntrenadas");

            migrationBuilder.RenameTable(
                name: "Entrenamiento",
                newName: "Entrenamientos");

            migrationBuilder.RenameTable(
                name: "EjercicioEntrenado",
                newName: "EjerciciosEntrenados");

            migrationBuilder.RenameIndex(
                name: "IX_SerieEntrenada_EjercicioEntrenadoId",
                table: "SeriesEntrenadas",
                newName: "IX_SeriesEntrenadas_EjercicioEntrenadoId");

            migrationBuilder.RenameIndex(
                name: "IX_Entrenamiento_UsuarioId_Fecha",
                table: "Entrenamientos",
                newName: "IX_Entrenamientos_UsuarioId_Fecha");

            migrationBuilder.RenameIndex(
                name: "IX_EjercicioEntrenado_EntrenamientoId",
                table: "EjerciciosEntrenados",
                newName: "IX_EjerciciosEntrenados_EntrenamientoId");

            migrationBuilder.RenameIndex(
                name: "IX_EjercicioEntrenado_EjercicioId",
                table: "EjerciciosEntrenados",
                newName: "IX_EjerciciosEntrenados_EjercicioId");

            migrationBuilder.AddColumn<int>(
                name: "RutinaActivaId",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "Entrenamientos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaActualizacion",
                table: "Entrenamientos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SeriesEntrenadas",
                table: "SeriesEntrenadas",
                column: "SerieEntrenadaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Entrenamientos",
                table: "Entrenamientos",
                column: "EntrenamientoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EjerciciosEntrenados",
                table: "EjerciciosEntrenados",
                column: "EjercicioEntrenadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RutinaActivaId",
                table: "Usuarios",
                column: "RutinaActivaId");

            migrationBuilder.AddForeignKey(
                name: "FK_EjerciciosEntrenados_Ejercicios_EjercicioId",
                table: "EjerciciosEntrenados",
                column: "EjercicioId",
                principalTable: "Ejercicios",
                principalColumn: "EjercicioId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EjerciciosEntrenados_Entrenamientos_EntrenamientoId",
                table: "EjerciciosEntrenados",
                column: "EntrenamientoId",
                principalTable: "Entrenamientos",
                principalColumn: "EntrenamientoId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Entrenamientos_Usuarios_UsuarioId",
                table: "Entrenamientos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "UsuarioId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SeriesEntrenadas_EjerciciosEntrenados_EjercicioEntrenadoId",
                table: "SeriesEntrenadas",
                column: "EjercicioEntrenadoId",
                principalTable: "EjerciciosEntrenados",
                principalColumn: "EjercicioEntrenadoId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Rutinas_RutinaActivaId",
                table: "Usuarios",
                column: "RutinaActivaId",
                principalTable: "Rutinas",
                principalColumn: "RutinaId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EjerciciosEntrenados_Ejercicios_EjercicioId",
                table: "EjerciciosEntrenados");

            migrationBuilder.DropForeignKey(
                name: "FK_EjerciciosEntrenados_Entrenamientos_EntrenamientoId",
                table: "EjerciciosEntrenados");

            migrationBuilder.DropForeignKey(
                name: "FK_Entrenamientos_Usuarios_UsuarioId",
                table: "Entrenamientos");

            migrationBuilder.DropForeignKey(
                name: "FK_SeriesEntrenadas_EjerciciosEntrenados_EjercicioEntrenadoId",
                table: "SeriesEntrenadas");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Rutinas_RutinaActivaId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_RutinaActivaId",
                table: "Usuarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SeriesEntrenadas",
                table: "SeriesEntrenadas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Entrenamientos",
                table: "Entrenamientos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EjerciciosEntrenados",
                table: "EjerciciosEntrenados");

            migrationBuilder.DropColumn(
                name: "RutinaActivaId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Entrenamientos");

            migrationBuilder.DropColumn(
                name: "UltimaActualizacion",
                table: "Entrenamientos");

            migrationBuilder.RenameTable(
                name: "SeriesEntrenadas",
                newName: "SerieEntrenada");

            migrationBuilder.RenameTable(
                name: "Entrenamientos",
                newName: "Entrenamiento");

            migrationBuilder.RenameTable(
                name: "EjerciciosEntrenados",
                newName: "EjercicioEntrenado");

            migrationBuilder.RenameIndex(
                name: "IX_SeriesEntrenadas_EjercicioEntrenadoId",
                table: "SerieEntrenada",
                newName: "IX_SerieEntrenada_EjercicioEntrenadoId");

            migrationBuilder.RenameIndex(
                name: "IX_Entrenamientos_UsuarioId_Fecha",
                table: "Entrenamiento",
                newName: "IX_Entrenamiento_UsuarioId_Fecha");

            migrationBuilder.RenameIndex(
                name: "IX_EjerciciosEntrenados_EntrenamientoId",
                table: "EjercicioEntrenado",
                newName: "IX_EjercicioEntrenado_EntrenamientoId");

            migrationBuilder.RenameIndex(
                name: "IX_EjerciciosEntrenados_EjercicioId",
                table: "EjercicioEntrenado",
                newName: "IX_EjercicioEntrenado_EjercicioId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SerieEntrenada",
                table: "SerieEntrenada",
                column: "SerieEntrenadaId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Entrenamiento",
                table: "Entrenamiento",
                column: "EntrenamientoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EjercicioEntrenado",
                table: "EjercicioEntrenado",
                column: "EjercicioEntrenadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_EjercicioEntrenado_Ejercicios_EjercicioId",
                table: "EjercicioEntrenado",
                column: "EjercicioId",
                principalTable: "Ejercicios",
                principalColumn: "EjercicioId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EjercicioEntrenado_Entrenamiento_EntrenamientoId",
                table: "EjercicioEntrenado",
                column: "EntrenamientoId",
                principalTable: "Entrenamiento",
                principalColumn: "EntrenamientoId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Entrenamiento_Usuarios_UsuarioId",
                table: "Entrenamiento",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "UsuarioId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SerieEntrenada_EjercicioEntrenado_EjercicioEntrenadoId",
                table: "SerieEntrenada",
                column: "EjercicioEntrenadoId",
                principalTable: "EjercicioEntrenado",
                principalColumn: "EjercicioEntrenadoId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
