using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vetcare.Application.Reportes;

namespace Vetcare.API.Controllers;

[ApiController]
[Route("api/reportes")]
public sealed class ReportesController(ISender sender) : ControllerBase
{
    [HttpGet("atenciones")]
    public async Task<IActionResult> Atenciones(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? veterinario,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetReporteAtencionesQuery(desde, hasta, veterinario), cancellationToken);
        return result.StatusCode == 200
            ? Ok(result.Value)
            : this.ToActionResult(result);
    }
}
