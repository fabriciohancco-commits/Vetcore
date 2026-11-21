using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vetcare.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Medicamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    PrincipioActivo = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    Presentacion = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    RequiereReceta = table.Column<bool>(type: "INTEGER", nullable: false),
                    StockUnidades = table.Column<int>(type: "INTEGER", nullable: false),
                    StockMinimo = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicamentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Propietarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombres = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Apellidos = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false),
                    Correo = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 180, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Propietarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Correo = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Rol = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosSistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mascotas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Especie = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Raza = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    FechaNacimiento = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    PesoKg = table.Column<decimal>(type: "TEXT", nullable: true),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PropietarioId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mascotas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mascotas_Propietarios_PropietarioId",
                        column: x => x.PropietarioId,
                        principalTable: "Propietarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PropietarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    MascotaId = table.Column<int>(type: "INTEGER", nullable: false),
                    VeterinarioNombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DuracionMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    Motivo = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Citas_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Citas_Propietarios_PropietarioId",
                        column: x => x.PropietarioId,
                        principalTable: "Propietarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsultasMedicas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MascotaId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaConsulta = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VeterinarioNombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Motivo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Diagnostico = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Tratamiento = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    VacunasAplicadas = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultasMedicas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultasMedicas_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Operaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MascotaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Procedimiento = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false),
                    VeterinarioNombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    FechaProgramada = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Notas = table.Column<string>(type: "TEXT", maxLength: 800, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operaciones_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prescripciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MascotaId = table.Column<int>(type: "INTEGER", nullable: false),
                    MedicamentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VeterinarioNombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    NumeroReceta = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    Dosis = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Frecuencia = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Duracion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Indicaciones = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Notas = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescripciones_Mascotas_MascotaId",
                        column: x => x.MascotaId,
                        principalTable: "Mascotas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescripciones_Medicamentos_MedicamentoId",
                        column: x => x.MedicamentoId,
                        principalTable: "Medicamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citas_MascotaId",
                table: "Citas",
                column: "MascotaId");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_PropietarioId",
                table: "Citas",
                column: "PropietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultasMedicas_MascotaId",
                table: "ConsultasMedicas",
                column: "MascotaId");

            migrationBuilder.CreateIndex(
                name: "IX_Mascotas_PropietarioId",
                table: "Mascotas",
                column: "PropietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Operaciones_MascotaId",
                table: "Operaciones",
                column: "MascotaId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescripciones_MascotaId",
                table: "Prescripciones",
                column: "MascotaId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescripciones_MedicamentoId",
                table: "Prescripciones",
                column: "MedicamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSistema_Correo",
                table: "UsuariosSistema",
                column: "Correo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "ConsultasMedicas");

            migrationBuilder.DropTable(
                name: "Operaciones");

            migrationBuilder.DropTable(
                name: "Prescripciones");

            migrationBuilder.DropTable(
                name: "UsuariosSistema");

            migrationBuilder.DropTable(
                name: "Mascotas");

            migrationBuilder.DropTable(
                name: "Medicamentos");

            migrationBuilder.DropTable(
                name: "Propietarios");
        }
    }
}
