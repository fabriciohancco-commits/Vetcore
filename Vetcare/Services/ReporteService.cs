using Microsoft.EntityFrameworkCore;
using Vetcare.Data;

namespace Vetcare.Services;

public sealed class ReporteService(VetCareDbContext db)
{
    public async Task<List<ReporteVeterinarioDto>> ObtenerReporteAsync(
        DateTime? desde,
        DateTime? hasta,
        string? veterinario)
    {
        var consulta = db.ConsultasMedicas
            .AsNoTracking()
            .AsQueryable();

        if (desde.HasValue)
        {
            consulta = consulta.Where(c =>
                c.FechaConsulta >= desde.Value.Date);
        }

        if (hasta.HasValue)
        {
            var limite = hasta.Value.Date.AddDays(1);

            consulta = consulta.Where(c =>
                c.FechaConsulta < limite);
        }

        if (!string.IsNullOrWhiteSpace(veterinario))
        {
            consulta = consulta.Where(c =>
                c.VeterinarioNombre.Contains(veterinario));
        }

        return await consulta
            .GroupBy(c => c.VeterinarioNombre)
            .Select(grupo => new ReporteVeterinarioDto
            {
                Veterinario = grupo.Key,
                TotalAtenciones = grupo.Count()
            })
            .OrderByDescending(r => r.TotalAtenciones)
            .ToListAsync();
    }
}

public sealed class ReporteVeterinarioDto
{
    public string Veterinario { get; set; } = string.Empty;

    public int TotalAtenciones { get; set; }
}