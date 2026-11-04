using Microsoft.EntityFrameworkCore;
using Vetcare.Models;

namespace Vetcare.Data;

public sealed class VetCareDbContext(
    DbContextOptions<VetCareDbContext> options) : DbContext(options)
{
    public DbSet<Propietario> Propietarios => Set<Propietario>();

    public DbSet<Mascota> Mascotas => Set<Mascota>();

    public DbSet<ConsultaMedica> ConsultasMedicas => Set<ConsultaMedica>();

    public DbSet<ProductoInventario> ProductosInventario => Set<ProductoInventario>();

    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

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

        modelBuilder.Entity<ProductoInventario>()
            .Property(p => p.Codigo)
            .UseCollation("NOCASE");

        modelBuilder.Entity<ProductoInventario>()
            .HasIndex(p => p.Codigo)
            .IsUnique();

        modelBuilder.Entity<MovimientoInventario>()
            .HasOne(m => m.ProductoInventario)
            .WithMany()
            .HasForeignKey(m => m.ProductoInventarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoInventario>()
            .HasIndex(m => new { m.ProductoInventarioId, m.FechaMovimientoUtc });
    }
}
