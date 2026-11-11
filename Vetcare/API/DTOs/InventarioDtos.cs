using System.ComponentModel.DataAnnotations;
using Vetcare.Domain.Entities;

namespace Vetcare.API.DTOs;

public sealed record ProductoInventarioDto(int Id, string Codigo, string Nombre, string Categoria,
    string UnidadMedida, decimal StockActual, decimal StockMinimo, bool Activo, DateTime ActualizadoUtc);
public sealed record MovimientoInventarioDto(int Id, int ProductoInventarioId, string Producto, string Codigo,
    string Tipo, decimal Cantidad, string Motivo, DateTime FechaMovimientoUtc);

public sealed class CrearProductoInventarioRequestDto
{
    [Required, StringLength(30, MinimumLength = 2)] public string Codigo { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 2)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(30)] public string Categoria { get; set; } = string.Empty;
    [Required, StringLength(20)] public string UnidadMedida { get; set; } = string.Empty;
    [Range(typeof(decimal), "0", "1000000")] public decimal StockInicial { get; set; }
    [Range(typeof(decimal), "0", "1000000")] public decimal StockMinimo { get; set; }
}

public sealed class ActualizarProductoInventarioRequestDto
{
    [Required, StringLength(30, MinimumLength = 2)] public string Codigo { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 2)] public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(30)] public string Categoria { get; set; } = string.Empty;
    [Required, StringLength(20)] public string UnidadMedida { get; set; } = string.Empty;
    [Range(typeof(decimal), "0", "1000000")] public decimal StockMinimo { get; set; }
}

public sealed class RegistrarMovimientoInventarioRequestDto
{
    [Range(1, int.MaxValue)] public int ProductoInventarioId { get; set; }
    [Required, StringLength(10)] public string Tipo { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.01", "1000000")] public decimal Cantidad { get; set; }
    [Required, StringLength(200, MinimumLength = 2)] public string Motivo { get; set; } = string.Empty;
}

public sealed record CambiarEstadoProductoRequestDto(bool Activo);

internal static class InventarioDtoMappings
{
    public static ProductoInventarioDto ToDto(this ProductoInventario value) => new(value.Id, value.Codigo,
        value.Nombre, value.Categoria, value.UnidadMedida, value.StockActual, value.StockMinimo, value.Activo, value.ActualizadoUtc);

    public static MovimientoInventarioDto ToDto(this MovimientoInventario value) => new(value.Id,
        value.ProductoInventarioId, value.ProductoInventario?.Nombre ?? "", value.ProductoInventario?.Codigo ?? "",
        value.Tipo, value.Cantidad, value.Motivo, value.FechaMovimientoUtc);
}
