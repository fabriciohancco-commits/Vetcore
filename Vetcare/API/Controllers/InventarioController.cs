using MediatR;
using Microsoft.AspNetCore.Mvc;
using Vetcare.API.DTOs;
using Vetcare.Application.Common;
using Vetcare.Application.Inventario;

namespace Vetcare.API.Controllers;

[ApiController]
[Route("api/inventario")]
public sealed class InventarioController(ISender sender) : ControllerBase
{
    [HttpGet("productos")]
    public async Task<IActionResult> Productos([FromQuery] bool incluirInactivos = false, CancellationToken ct = default)
    {
        var items = await sender.Send(new ListProductosInventarioQuery(incluirInactivos), ct);
        return Ok(items.Select(x => x.ToDto()));
    }

    [HttpGet("productos/bajo-stock")]
    public async Task<IActionResult> BajoStock(CancellationToken ct) =>
        Ok((await sender.Send(new ListarBajoStockQuery(), ct)).Select(x => x.ToDto()));

    [HttpPost("productos")]
    public async Task<IActionResult> CrearProducto(CrearProductoInventarioRequestDto input, CancellationToken ct)
    {
        var result = await sender.Send(new CrearProductoCommand(input.Codigo, input.Nombre, input.Categoria,
            input.UnidadMedida, input.StockInicial, input.StockMinimo), ct);
        return result.StatusCode == 201
            ? Created($"/api/inventario/productos/{result.Value!.Id}", result.Value.ToDto())
            : this.ToActionResult(result);
    }

    [HttpPut("productos/{id:int}")]
    public async Task<IActionResult> ActualizarProducto(int id, ActualizarProductoInventarioRequestDto input, CancellationToken ct)
    {
        var result = await sender.Send(new ActualizarProductoCommand(id, input.Codigo, input.Nombre,
            input.Categoria, input.UnidadMedida, input.StockMinimo), ct);
        return result.StatusCode == 200 ? Ok(result.Value!.ToDto()) : this.ToActionResult(result);
    }

    [HttpPut("productos/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoProductoRequestDto input, CancellationToken ct)
    {
        var result = await sender.Send(new CambiarEstadoProductoCommand(id, input.Activo), ct);
        return result.StatusCode == 200 ? Ok(result.Value!.ToDto()) : this.ToActionResult(result);
    }

    [HttpDelete("productos/{id:int}")]
    public async Task<IActionResult> EliminarProducto(int id, CancellationToken ct)
    {
        var result = await sender.Send(new CambiarEstadoProductoCommand(id, false), ct);
        return result.StatusCode == 200 ? NoContent() : this.ToActionResult(result);
    }

    [HttpGet("movimientos")]
    public async Task<IActionResult> Movimientos([FromQuery] int? productoId, CancellationToken ct) =>
        Ok((await sender.Send(new ListarMovimientosQuery(productoId), ct)).Select(x => x.ToDto()));

    [HttpPost("movimientos")]
    public async Task<IActionResult> RegistrarMovimiento(RegistrarMovimientoInventarioRequestDto input, CancellationToken ct)
    {
        var result = await sender.Send(new RegistrarMovimientoCommand(input.ProductoInventarioId, input.Tipo,
            input.Cantidad, input.Motivo), ct);
        return result.StatusCode == 201
            ? Created($"/api/inventario/movimientos/{result.Value!.Id}", result.Value.ToDto())
            : this.ToActionResult(result);
    }
}
