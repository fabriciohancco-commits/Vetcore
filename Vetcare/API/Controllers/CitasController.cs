using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vetcare.API.DTOs;
using Vetcare.Application.UseCases.Citas.Commands;
using Vetcare.Application.UseCases.Citas.Queries;

namespace Vetcare.API.Controllers;

[Authorize(Roles = "Administrador,Veterinario,Recepcionista")]
[ApiController]
[Route("api/citas")]
public sealed class CitasController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CitaDto>>> Listar(CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListCitasQuery(), cancellationToken)).Select(cita => cita.ToDto()).ToList());

    [HttpPost]
    public async Task<IActionResult> Crear(CitaRequestDto input, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateCitaCommand(input.ToEntity()), cancellationToken);
        return result.StatusCode == 201
            ? Created($"/api/citas/{result.Value!.Id}", result.Value.ToDto())
            : this.ToActionResult(result);
    }

    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> ActualizarEstado(
        int id, UpdateEstadoCitaDto input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new UpdateEstadoCitaCommand(id, input.Estado), cancellationToken));
}
