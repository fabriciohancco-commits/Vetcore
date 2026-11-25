using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vetcare.API.DTOs;
using Vetcare.Application.UseCases.Operaciones.Commands;
using Vetcare.Application.UseCases.Operaciones.Queries;

namespace Vetcare.API.Controllers;

[ApiController]
[Route("api/operaciones")]
public sealed class OperacionesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<OperacionDto>>> Listar(CancellationToken cancellationToken) =>
        Ok((await sender.Send(new ListOperacionesQuery(), cancellationToken)).Select(x => x.ToDto()).ToList());

    [HttpPost]
    public async Task<IActionResult> Crear(OperacionRequestDto input, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateOperacionCommand(input.ToEntity()), cancellationToken);
        return result.StatusCode == 201
            ? Created($"/api/operaciones/{result.Value!.Id}", result.Value.ToDto())
            : this.ToActionResult(result);
    }

    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> ActualizarEstado(int id, UpdateEstadoOperacionDto input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new UpdateEstadoOperacionCommand(id, input.Estado), cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new DeleteOperacionCommand(id), cancellationToken));
}
