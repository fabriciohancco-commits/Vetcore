using System.ComponentModel.DataAnnotations;

namespace Vetcare.Domain.Entities;

public sealed class Medicamento
{
    public int Id { get; set; }

    [Required, StringLength(120, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(120)]
    public string? PrincipioActivo { get; set; }

    [Required, StringLength(80)]
    public string Presentacion { get; set; } = string.Empty;

    public bool RequiereReceta { get; set; }

    [Range(0, 100000)]
    public int StockUnidades { get; set; }

    [Range(0, 100000)]
    public int StockMinimo { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    public bool Activo { get; set; } = true;
}
