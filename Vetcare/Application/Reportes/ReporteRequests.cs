using MediatR;
using Vetcare.Application.Common;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.Application.Reportes;

public sealed record GetReporteAtencionesQuery(DateTime? Desde, DateTime? Hasta, string? Veterinario)
    : IRequest<OperationResult<List<ReporteVeterinarioDto>>>;

public sealed class ReporteVeterinarioDto
{
    public string Veterinario { get; set; } = string.Empty;
    public int TotalAtenciones { get; set; }
}

internal sealed class GetReporteAtencionesHandler(IConsultaMedicaRepository consultas)
    : IRequestHandler<GetReporteAtencionesQuery, OperationResult<List<ReporteVeterinarioDto>>>
{
    public async Task<OperationResult<List<ReporteVeterinarioDto>>> Handle(
        GetReporteAtencionesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Desde.HasValue && request.Hasta.HasValue
            && request.Desde.Value.Date > request.Hasta.Value.Date)
            return new(400, Message: "La fecha inicial no puede ser posterior a la fecha final.");

        var rows = await consultas.GetAtencionesPorVeterinarioAsync(
            request.Desde,
            request.Hasta,
            request.Veterinario,
            cancellationToken);
        var report = rows.Select(row => new ReporteVeterinarioDto
        {
            Veterinario = row.Veterinario,
            TotalAtenciones = row.TotalAtenciones,
        }).ToList();

        return new(200, report);
    }
}
