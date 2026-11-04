using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vetcare.API.DTOs;
using Vetcare.Application.Consultas;
using Vetcare.Application.Mascotas;

namespace Vetcare.API.Controllers;

[ApiController]
[Route("api/mascotas")]
public sealed class MascotasController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MascotaDto>>> Listar(CancellationToken cancellationToken)
    {
        var mascotas = await sender.Send(new ListMascotasQuery(), cancellationToken);
        return Ok(mascotas.Select(mascota => mascota.ToDto()).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        MascotaRequestDto input, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateMascotaCommand(input.ToEntity()), cancellationToken);
        return result.StatusCode == 201
            ? Created($"/api/mascotas/{result.Value!.Id}", result.Value.ToDto())
            : this.ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id, MascotaRequestDto input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new UpdateMascotaCommand(id, input.ToEntity()), cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new DeleteMascotaCommand(id), cancellationToken));

    [HttpGet("{mascotaId:int}/historial")]
    public async Task<IActionResult> Historial(int mascotaId, CancellationToken cancellationToken)
    {
        var historial = await sender.Send(new GetHistorialMascotaQuery(mascotaId), cancellationToken);
        return historial is null
            ? NotFound(new { message = "Mascota no encontrada." })
            : Ok(historial.Consultas.Select(consulta => consulta.ToDto()).ToList());
    }
}
