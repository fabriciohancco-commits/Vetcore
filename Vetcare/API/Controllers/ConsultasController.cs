using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vetcare.API.DTOs;
using Vetcare.Application.Consultas;

namespace Vetcare.API.Controllers;

[ApiController]
[Route("api/consultas")]
public sealed class ConsultasController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ConsultaMedicaDto>>> Listar(CancellationToken cancellationToken)
    {
        var consultas = await sender.Send(new ListConsultasQuery(), cancellationToken);
        return Ok(consultas.Select(consulta => consulta.ToDto()).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id, CancellationToken cancellationToken)
    {
        var consulta = await sender.Send(new GetConsultaQuery(id), cancellationToken);
        return consulta is null ? NotFound() : Ok(consulta.ToDto());
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        ConsultaMedicaRequestDto input, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateConsultaCommand(input.ToEntity()), cancellationToken);
        return result.StatusCode == 201
            ? Created($"/api/consultas/{result.Value!.Id}", result.Value.ToDto())
            : this.ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id, ConsultaMedicaRequestDto input, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(
            new UpdateConsultaCommand(id, input.ToEntity()), cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken) =>
        this.ToActionResult(await sender.Send(new DeleteConsultaCommand(id), cancellationToken));
}
