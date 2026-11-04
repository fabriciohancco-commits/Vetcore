using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vetcare.API.DTOs;
using Vetcare.Application.Propietarios;

namespace Vetcare.API.Controllers;

[ApiController]
[Route("api/propietarios")]
public sealed class PropietariosController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PropietarioDto>>> Listar(CancellationToken cancellationToken)
    {
        var propietarios = await sender.Send(new ListPropietariosQuery(), cancellationToken);
        return Ok(propietarios.Select(propietario => propietario.ToDto()).ToList());
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        PropietarioRequestDto input, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreatePropietarioCommand(input.ToEntity()), cancellationToken);
        return result.StatusCode == 201
            ? Created($"/api/propietarios/{result.Value!.Id}", result.Value.ToDto())
            : this.ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id, PropietarioRequestDto input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new UpdatePropietarioCommand(id, input.ToEntity()), cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new DeletePropietarioCommand(id), cancellationToken));
}
