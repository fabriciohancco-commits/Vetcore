using System.ComponentModel.DataAnnotations;

namespace Vetcare.Domain.Entities;

public sealed class Mascota
{
    public int Id { get; set; }

    [Required, StringLength(80, MinimumLength = 2)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Especie { get; set; } = string.Empty;

    [StringLength(60)]
    public string? Raza { get; set; }

    public DateOnly? FechaNacimiento { get; set; }

    [Range(0.01, 500)]
    public decimal? PesoKg { get; set; }

    [StringLength(500)]
    public string? Observaciones { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un propietario.")]
    public int PropietarioId { get; set; }
}
