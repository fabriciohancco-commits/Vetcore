using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetcare.Domain.Ports.Repositories;
using Vetcare.Infrastructure.Adapters.Repositories.Implementation;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Infrastructure;

public static class InfrastructureServicesExtensions
{
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

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<VetCareDbContext>();
        await db.Database.EnsureCreatedAsync();
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
                Dosis TEXT NOT NULL,
                Frecuencia TEXT NOT NULL,
                Duracion TEXT NOT NULL,
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
            CREATE INDEX IF NOT EXISTS IX_Prescripciones_MascotaId ON Prescripciones (MascotaId);
            CREATE INDEX IF NOT EXISTS IX_Prescripciones_MedicamentoId ON Prescripciones (MedicamentoId);
            CREATE INDEX IF NOT EXISTS IX_Operaciones_MascotaId ON Operaciones (MascotaId);
            """);

        await db.Database.OpenConnectionAsync();
        try
        {
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            await using (var command = db.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "PRAGMA table_info('Prescripciones');";
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync()) existingColumns.Add(reader.GetString(1));
            }

            foreach (var column in new[] { "Dosis", "Frecuencia", "Duracion" })
            {
                if (!existingColumns.Contains(column))
                    await db.Database.ExecuteSqlRawAsync(
                        $"ALTER TABLE Prescripciones ADD COLUMN {column} TEXT NOT NULL DEFAULT ''; ");
            }
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }
}
