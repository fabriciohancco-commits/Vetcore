using Microsoft.EntityFrameworkCore;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.Infrastructure.Persistence;

public sealed class VetCareDbContext(
    DbContextOptions<VetCareDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Propietario> Propietarios => Set<Propietario>();

    public DbSet<Mascota> Mascotas => Set<Mascota>();

    public DbSet<ConsultaMedica> ConsultasMedicas =>
        Set<ConsultaMedica>();

    public DbSet<Medicamento> Medicamentos => Set<Medicamento>();

    public DbSet<Prescripcion> Prescripciones => Set<Prescripcion>();

    public DbSet<Operacion> Operaciones => Set<Operacion>();

    public DbSet<Cita> Citas => Set<Cita>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Propietario>()
            .HasMany<Mascota>()
            .WithOne()
            .HasForeignKey(m => m.PropietarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConsultaMedica>()
            .HasOne(c => c.Mascota)
            .WithMany()
            .HasForeignKey(c => c.MascotaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Prescripcion>()
            .HasOne(p => p.Mascota)
            .WithMany()
            .HasForeignKey(p => p.MascotaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Prescripcion>()
            .HasOne(p => p.Medicamento)
            .WithMany()
            .HasForeignKey(p => p.MedicamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Operacion>()
            .HasOne(o => o.Mascota)
            .WithMany()
            .HasForeignKey(o => o.MascotaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cita>()
            .HasOne(c => c.Propietario)
            .WithMany()
            .HasForeignKey(c => c.PropietarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cita>()
            .HasOne(c => c.Mascota)
            .WithMany()
            .HasForeignKey(c => c.MascotaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
