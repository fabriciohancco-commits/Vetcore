using System.ComponentModel.DataAnnotations;

namespace Vetcare.Models;

public sealed class CrearProductoInventarioRequest
{
    [Required, StringLength(30, MinimumLength = 2)]
    public string Codigo { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Categoria { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string UnidadMedida { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "1000000")]
    public decimal StockInicial { get; set; }

    [Range(typeof(decimal), "0", "1000000")]
    public decimal StockMinimo { get; set; }
}

public sealed class ActualizarProductoInventarioRequest
{
    [Required, StringLength(30, MinimumLength = 2)]
    public string Codigo { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Categoria { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string UnidadMedida { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "1000000")]
    public decimal StockMinimo { get; set; }
}

public sealed class RegistrarMovimientoInventarioRequest
{
    [Range(1, int.MaxValue)]
    public int ProductoInventarioId { get; set; }

    [Required, StringLength(10)]
    public string Tipo { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Cantidad { get; set; }

    [Required, StringLength(200, MinimumLength = 2)]
    public string Motivo { get; set; } = string.Empty;
}

public sealed class CambiarEstadoProductoRequest
{
    public bool Activo { get; set; }
}
