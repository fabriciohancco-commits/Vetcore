using Microsoft.EntityFrameworkCore;

namespace Vetcare.Infrastructure.Persistence;

public static class VetCareDatabaseInitializer
{
    public static void Initialize(VetCareDbContext db)
    {
        db.Database.EnsureCreated();

        // EnsureCreated no altera bases ya existentes: estas tablas se agregan
        // de forma idempotente sin borrar los registros guardados.
        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "ConsultasMedicas" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_ConsultasMedicas" PRIMARY KEY AUTOINCREMENT,
                "MascotaId" INTEGER NOT NULL, "FechaConsulta" TEXT NOT NULL,
                "VeterinarioNombre" TEXT NOT NULL, "Motivo" TEXT NOT NULL,
                "Diagnostico" TEXT NOT NULL, "Tratamiento" TEXT NULL, "VacunasAplicadas" TEXT NULL,
                CONSTRAINT "FK_ConsultasMedicas_Mascotas_MascotaId"
                    FOREIGN KEY ("MascotaId") REFERENCES "Mascotas" ("Id") ON DELETE RESTRICT
            )
            """);
        db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS \"IX_ConsultasMedicas_MascotaId\" ON \"ConsultasMedicas\" (\"MascotaId\")");
        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "ProductosInventario" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_ProductosInventario" PRIMARY KEY AUTOINCREMENT,
                "Codigo" TEXT COLLATE NOCASE NOT NULL, "Nombre" TEXT NOT NULL, "Categoria" TEXT NOT NULL,
                "UnidadMedida" TEXT NOT NULL, "StockActual" TEXT NOT NULL, "StockMinimo" TEXT NOT NULL,
                "Activo" INTEGER NOT NULL, "ActualizadoUtc" TEXT NOT NULL
            )
            """);
        db.Database.ExecuteSqlRaw("CREATE UNIQUE INDEX IF NOT EXISTS \"IX_ProductosInventario_Codigo\" ON \"ProductosInventario\" (\"Codigo\")");
        db.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS "MovimientosInventario" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_MovimientosInventario" PRIMARY KEY AUTOINCREMENT,
                "ProductoInventarioId" INTEGER NOT NULL, "Tipo" TEXT NOT NULL, "Cantidad" TEXT NOT NULL,
                "Motivo" TEXT NOT NULL, "FechaMovimientoUtc" TEXT NOT NULL,
                CONSTRAINT "FK_MovimientosInventario_ProductosInventario_ProductoInventarioId"
                    FOREIGN KEY ("ProductoInventarioId") REFERENCES "ProductosInventario" ("Id") ON DELETE RESTRICT
            )
            """);
        db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS \"IX_MovimientosInventario_ProductoInventarioId_FechaMovimientoUtc\" ON \"MovimientosInventario\" (\"ProductoInventarioId\", \"FechaMovimientoUtc\")");
    }
}
