using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vetcare.API.DTOs;
using Vetcare.Application.UseCases.Farmacia.Commands;
using Vetcare.Application.UseCases.Farmacia.Queries;

namespace Vetcare.API.Controllers;

[Authorize(Roles = "Administrador,Veterinario,Recepcionista")]
[ApiController]
[Route("api/farmacia")]
public sealed class FarmaciaController(ISender sender) : ControllerBase
{
    [HttpGet("medicamentos")]
    public async Task<ActionResult<List<MedicamentoDto>>> Medicamentos(CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListMedicamentosQuery(), cancellationToken)).Select(x => x.ToDto()).ToList());

    [HttpPost("medicamentos")]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> CrearMedicamento(MedicamentoRequestDto input, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateMedicamentoCommand(input.ToEntity()), cancellationToken);
        return result.StatusCode == 201
            ? Created($"/api/farmacia/medicamentos/{result.Value!.Id}", result.Value.ToDto())
            : this.ToActionResult(result);
    }

    [HttpPut("medicamentos/{id:int}")]
    [Authorize(Roles = "Administrador,Recepcionista")]
    public async Task<IActionResult> ActualizarMedicamento(int id, MedicamentoRequestDto input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new UpdateMedicamentoCommand(id, input.ToEntity()), cancellationToken));

    [HttpGet("prescripciones")]
    [Authorize(Roles = "Administrador,Veterinario")]
    public async Task<ActionResult<List<PrescripcionDto>>> Prescripciones([FromQuery] int? mascotaId, CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListPrescripcionesQuery(mascotaId), cancellationToken)).Select(x => x.ToDto()).ToList());

    [HttpPost("prescripciones")]
    [Authorize(Roles = "Administrador,Veterinario")]
    public async Task<IActionResult> CrearPrescripcion(PrescripcionRequestDto input, CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(new CreatePrescripcionCommand(input.ToEntity()), cancellationToken);
            return result.StatusCode == 201
                ? Created($"/api/farmacia/prescripciones/{result.Value!.Id}", result.Value.ToDto())
                : this.ToActionResult(result);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "El inventario cambió durante la operación. Actualiza los datos e inténtalo nuevamente." });
        }
    }
}
