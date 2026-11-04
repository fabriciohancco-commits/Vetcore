using System.ComponentModel.DataAnnotations;

namespace Vetcare.Models;

public sealed class ConsultaMedica
{
    public int Id { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int MascotaId { get; set; }

    public DateTime FechaConsulta { get; set; } = DateTime.Now;

    [Required, StringLength(150)]
    public string VeterinarioNombre { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Motivo { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Diagnostico { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Tratamiento { get; set; }

    [StringLength(1000)]
    public string? VacunasAplicadas { get; set; }

    public Mascota? Mascota { get; set; }
}