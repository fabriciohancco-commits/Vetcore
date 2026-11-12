using System.ComponentModel.DataAnnotations;

namespace Vetcare.Models;

public sealed class Medicamento
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descripcion { get; set; }

    [Required, StringLength(50)]
    public string Presentacion { get; set; } = string.Empty;

    [Range(0, 100000)]
    public int Stock { get; set; }

    [Required, StringLength(20)]
    public string UnidadMedida { get; set; } = string.Empty;

    public DateTime? FechaVencimiento { get; set; }

    [Range(0, 1000000)]
    public decimal Precio { get; set; }
}