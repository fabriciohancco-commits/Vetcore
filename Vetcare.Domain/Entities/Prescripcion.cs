using System.ComponentModel.DataAnnotations;

namespace Vetcare.Domain.Entities;

/// <summary>Records veterinarian-entered directions; it never generates a treatment plan.</summary>
public sealed class Prescripcion
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int MascotaId { get; set; }

    [Range(1, int.MaxValue)]
    public int MedicamentoId { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    [Required, StringLength(150)]
    public string VeterinarioNombre { get; set; } = string.Empty;

    [StringLength(60)]
    public string? NumeroReceta { get; set; }

    [Required, StringLength(100, MinimumLength = 1)]
    public string Dosis { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string Frecuencia { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string Duracion { get; set; } = string.Empty;

    [Required, StringLength(1000, MinimumLength = 3)]
    public string Indicaciones { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Notas { get; set; }

    public Mascota? Mascota { get; set; }
    public Medicamento? Medicamento { get; set; }
}
