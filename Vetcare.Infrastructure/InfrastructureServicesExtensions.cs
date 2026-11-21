using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetcare.Domain.Ports.Repositories;
using Vetcare.Infrastructure.Adapters.Repositories.Implementation;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Infrastructure;

public static class InfrastructureServicesExtensions
{
    private const string BaselineMigrationId = "20261125130051_InitialCreate";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<VetCareDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("VetCare") ?? "Data Source=vetcare.db"));
        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<VetCareDbContext>());
        services.AddScoped<IPropietarioRepository, PropietarioRepository>();
        services.AddScoped<IMascotaRepository, MascotaRepository>();
        services.AddScoped<IConsultaMedicaRepository, ConsultaMedicaRepository>();
        services.AddScoped<IMedicamentoRepository, MedicamentoRepository>();
        services.AddScoped<IPrescripcionRepository, PrescripcionRepository>();
        services.AddScoped<IOperacionRepository, OperacionRepository>();
        services.AddScoped<ICitaRepository, CitaRepository>();
        services.AddScoped<IUsuarioSistemaRepository, UsuarioSistemaRepository>();
        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<VetCareDbContext>();
        if (await HasLegacyTablesAsync(db))
        {
            await EnsureLegacySchemaAsync(db);
            await db.Database.ExecuteSqlRawAsync("""
                CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                    "ProductVersion" TEXT NOT NULL
                );
                INSERT OR IGNORE INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                VALUES ('20261125130051_InitialCreate', '8.0.22');
                """);
        }

        await db.Database.MigrateAsync();
    }

    private static async Task<bool> HasLegacyTablesAsync(VetCareDbContext db)
    {
        await db.Database.OpenConnectionAsync();
        try
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = """
                SELECT EXISTS(
                    SELECT 1 FROM sqlite_master
                    WHERE type = 'table'
                      AND name NOT LIKE 'sqlite_%'
                      AND name <> '__EFMigrationsHistory'
                );
                """;
            return Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    private static async Task EnsureLegacySchemaAsync(VetCareDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS Medicamentos (
                Id INTEGER NOT NULL CONSTRAINT PK_Medicamentos PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                PrincipioActivo TEXT NULL,
                Presentacion TEXT NOT NULL,
                RequiereReceta INTEGER NOT NULL,
                StockUnidades INTEGER NOT NULL,
                StockMinimo INTEGER NOT NULL,
                FechaVencimiento TEXT NULL,
                Activo INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS Prescripciones (
                Id INTEGER NOT NULL CONSTRAINT PK_Prescripciones PRIMARY KEY AUTOINCREMENT,
                MascotaId INTEGER NOT NULL,
                MedicamentoId INTEGER NOT NULL,
                FechaRegistro TEXT NOT NULL,
                VeterinarioNombre TEXT NOT NULL,
                NumeroReceta TEXT NULL,
                Dosis TEXT NOT NULL DEFAULT '',
                Frecuencia TEXT NOT NULL DEFAULT '',
                Duracion TEXT NOT NULL DEFAULT '',
                Indicaciones TEXT NOT NULL,
                Notas TEXT NULL,
                CONSTRAINT FK_Prescripciones_Mascotas_MascotaId FOREIGN KEY (MascotaId) REFERENCES Mascotas (Id) ON DELETE RESTRICT,
                CONSTRAINT FK_Prescripciones_Medicamentos_MedicamentoId FOREIGN KEY (MedicamentoId) REFERENCES Medicamentos (Id) ON DELETE RESTRICT
            );
            CREATE TABLE IF NOT EXISTS Operaciones (
                Id INTEGER NOT NULL CONSTRAINT PK_Operaciones PRIMARY KEY AUTOINCREMENT,
                MascotaId INTEGER NOT NULL,
                Procedimiento TEXT NOT NULL,
                VeterinarioNombre TEXT NOT NULL,
                FechaProgramada TEXT NOT NULL,
                Estado TEXT NOT NULL,
                Notas TEXT NULL,
                CONSTRAINT FK_Operaciones_Mascotas_MascotaId FOREIGN KEY (MascotaId) REFERENCES Mascotas (Id) ON DELETE RESTRICT
            );
            CREATE TABLE IF NOT EXISTS Citas (
                Id INTEGER NOT NULL CONSTRAINT PK_Citas PRIMARY KEY AUTOINCREMENT,
                PropietarioId INTEGER NOT NULL,
                MascotaId INTEGER NOT NULL,
                VeterinarioNombre TEXT NOT NULL,
                FechaHora TEXT NOT NULL,
                DuracionMinutos INTEGER NOT NULL,
                Motivo TEXT NOT NULL,
                Estado TEXT NOT NULL,
                CONSTRAINT FK_Citas_Propietarios_PropietarioId FOREIGN KEY (PropietarioId) REFERENCES Propietarios (Id) ON DELETE RESTRICT,
                CONSTRAINT FK_Citas_Mascotas_MascotaId FOREIGN KEY (MascotaId) REFERENCES Mascotas (Id) ON DELETE RESTRICT
            );
            CREATE TABLE IF NOT EXISTS UsuariosSistema (
                Id INTEGER NOT NULL CONSTRAINT PK_UsuariosSistema PRIMARY KEY AUTOINCREMENT,
                Correo TEXT NOT NULL COLLATE NOCASE,
                PasswordHash TEXT NOT NULL,
                Rol TEXT NOT NULL,
                Activo INTEGER NOT NULL,
                FechaCreacion TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS IX_Prescripciones_MascotaId ON Prescripciones (MascotaId);
            CREATE INDEX IF NOT EXISTS IX_Prescripciones_MedicamentoId ON Prescripciones (MedicamentoId);
            CREATE INDEX IF NOT EXISTS IX_Operaciones_MascotaId ON Operaciones (MascotaId);
            CREATE INDEX IF NOT EXISTS IX_Citas_VeterinarioNombre_FechaHora ON Citas (VeterinarioNombre, FechaHora);
            CREATE INDEX IF NOT EXISTS IX_Citas_PropietarioId ON Citas (PropietarioId);
            CREATE INDEX IF NOT EXISTS IX_Citas_MascotaId ON Citas (MascotaId);
            CREATE UNIQUE INDEX IF NOT EXISTS IX_UsuariosSistema_Correo ON UsuariosSistema (Correo);
            """);

        await db.Database.OpenConnectionAsync();
        try
        {
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var command = db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "PRAGMA table_info('Prescripciones');";
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync()) columns.Add(reader.GetString(1));
            }

            foreach (var (name, sql) in new[]
            {
                ("Dosis", "ALTER TABLE Prescripciones ADD COLUMN Dosis TEXT NOT NULL DEFAULT '';"),
                ("Frecuencia", "ALTER TABLE Prescripciones ADD COLUMN Frecuencia TEXT NOT NULL DEFAULT '';"),
                ("Duracion", "ALTER TABLE Prescripciones ADD COLUMN Duracion TEXT NOT NULL DEFAULT '';")
            })
            {
                if (columns.Contains(name)) continue;
                await using var command = db.Database.GetDbConnection().CreateCommand();
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
            }
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
