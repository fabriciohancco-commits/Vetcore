using Microsoft.EntityFrameworkCore;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Infrastructure.Adapters.Repositories.Implementation;

public sealed class ConsultaMedicaRepository(VetCareDbContext db) : IConsultaMedicaRepository
{
    public Task<List<ConsultaMedica>> ListAsync(CancellationToken cancellationToken = default) =>
        db.ConsultasMedicas.AsNoTracking()
            .Include(consulta => consulta.Mascota)
            .OrderByDescending(consulta => consulta.FechaConsulta)
            .ToListAsync(cancellationToken);

    public Task<ConsultaMedica?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.ConsultasMedicas.Include(consulta => consulta.Mascota)
            .FirstOrDefaultAsync(consulta => consulta.Id == id, cancellationToken);

    public Task<List<ConsultaMedica>> ListByMascotaAsync(
        int mascotaId,
        CancellationToken cancellationToken = default) =>
        db.ConsultasMedicas.AsNoTracking()
            .Where(consulta => consulta.MascotaId == mascotaId)
            .OrderByDescending(consulta => consulta.FechaConsulta)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsForMascotaAsync(int mascotaId, CancellationToken cancellationToken = default) =>
        db.ConsultasMedicas.AnyAsync(consulta => consulta.MascotaId == mascotaId, cancellationToken);

    public async Task<List<(string Veterinario, int TotalAtenciones)>> GetAtencionesPorVeterinarioAsync(
        DateTime? desde,
        DateTime? hasta,
        string? veterinario,
        CancellationToken cancellationToken = default)
    {
        var query = db.ConsultasMedicas.AsNoTracking().AsQueryable();
        if (desde.HasValue)
            query = query.Where(consulta => consulta.FechaConsulta >= desde.Value.Date);
        if (hasta.HasValue)
        {
            var endExclusive = hasta.Value.Date.AddDays(1);
            query = query.Where(consulta => consulta.FechaConsulta < endExclusive);
        }
        if (!string.IsNullOrWhiteSpace(veterinario))
            query = query.Where(consulta => consulta.VeterinarioNombre.Contains(veterinario));

        var totals = await query.GroupBy(consulta => consulta.VeterinarioNombre)
            .Select(group => new { Veterinario = group.Key, TotalAtenciones = group.Count() })
            .OrderByDescending(row => row.TotalAtenciones)
            .ToListAsync(cancellationToken);

        return totals.Select(row => (row.Veterinario, row.TotalAtenciones)).ToList();
    }

    public void Add(ConsultaMedica consulta) => db.ConsultasMedicas.Add(consulta);

    public void Remove(ConsultaMedica consulta) => db.ConsultasMedicas.Remove(consulta);
}
