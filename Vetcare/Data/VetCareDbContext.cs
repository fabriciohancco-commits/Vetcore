using Microsoft.EntityFrameworkCore;
using Vetcare.Models;

namespace Vetcare.Data;

public sealed class VetCareDbContext(DbContextOptions<VetCareDbContext> options) : DbContext(options)
{
    public DbSet<Propietario> Propietarios => Set<Propietario>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Propietario>()
            .HasMany<Mascota>()
            .WithOne()
            .HasForeignKey(m => m.PropietarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
