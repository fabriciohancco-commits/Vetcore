using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetcare.Application.Common;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Application.Reportes;

public sealed record GetReporteAtencionesQuery(DateTime? Desde, DateTime? Hasta, string? Veterinario)
    : IRequest<OperationResult<List<ReporteVeterinarioDto>>>;

public sealed class ReporteVeterinarioDto
{
    public string Veterinario { get; set; } = string.Empty;
    public int TotalAtenciones { get; set; }
}

internal sealed class GetReporteAtencionesHandler(VetCareDbContext db)
    : IRequestHandler<GetReporteAtencionesQuery, OperationResult<List<ReporteVeterinarioDto>>>
{
    public async Task<OperationResult<List<ReporteVeterinarioDto>>> Handle(
        GetReporteAtencionesQuery request, CancellationToken cancellationToken)
    {
        if (request.Desde.HasValue && request.Hasta.HasValue && request.Desde.Value.Date > request.Hasta.Value.Date)
            return new(400, Message: "La fecha inicial no puede ser posterior a la fecha final.");

        var consulta = db.ConsultasMedicas.AsNoTracking().AsQueryable();
        if (request.Desde.HasValue)
            consulta = consulta.Where(c => c.FechaConsulta >= request.Desde.Value.Date);
        if (request.Hasta.HasValue)
        {
            var limite = request.Hasta.Value.Date.AddDays(1);
            consulta = consulta.Where(c => c.FechaConsulta < limite);
        }
        if (!string.IsNullOrWhiteSpace(request.Veterinario))
            consulta = consulta.Where(c => c.VeterinarioNombre.Contains(request.Veterinario));

        var rows = await consulta.GroupBy(c => c.VeterinarioNombre)
            .Select(grupo => new ReporteVeterinarioDto
            {
                Veterinario = grupo.Key,
                TotalAtenciones = grupo.Count()
            })
            .OrderByDescending(row => row.TotalAtenciones)
            .ToListAsync(cancellationToken);
        return new(200, rows);
    }
}
