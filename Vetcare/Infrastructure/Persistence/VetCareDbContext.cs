using Microsoft.EntityFrameworkCore;
using Vetcare.Domain.Entities;

namespace Vetcare.Infrastructure.Persistence;

public sealed class VetCareDbContext(
    DbContextOptions<VetCareDbContext> options) : DbContext(options)
{
    public DbSet<Propietario> Propietarios => Set<Propietario>();

    public DbSet<Mascota> Mascotas => Set<Mascota>();

    public DbSet<ConsultaMedica> ConsultasMedicas =>
        Set<ConsultaMedica>();

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
    }
}
