using System.ComponentModel.DataAnnotations;

namespace Vetcare.Models;

public sealed class MovimientoInventario
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int ProductoInventarioId { get; set; }

    [Required, StringLength(10)]
    public string Tipo { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Cantidad { get; set; }

    [Required, StringLength(200, MinimumLength = 2)]
    public string Motivo { get; set; } = string.Empty;

    public DateTime FechaMovimientoUtc { get; set; } = DateTime.UtcNow;

    public ProductoInventario? ProductoInventario { get; set; }
}
