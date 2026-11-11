using System.ComponentModel.DataAnnotations;

namespace Vetcare.Domain.Entities;

public sealed class ProductoInventario
{
    public int Id { get; set; }
    [Required, StringLength(30, MinimumLength = 2)]
    public string Codigo { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;
    [Required, StringLength(30)]
    public string Categoria { get; set; } = string.Empty;
    [Required, StringLength(20)]
    public string UnidadMedida { get; set; } = string.Empty;
    [Range(typeof(decimal), "0", "1000000")]
    public decimal StockActual { get; set; }
    [Range(typeof(decimal), "0", "1000000")]
    public decimal StockMinimo { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime ActualizadoUtc { get; set; } = DateTime.UtcNow;
}
