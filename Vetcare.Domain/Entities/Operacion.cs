using System.ComponentModel.DataAnnotations;

namespace Vetcare.Domain.Entities;

public sealed class Operacion
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int MascotaId { get; set; }

    [Required, StringLength(140, MinimumLength = 3)]
    public string Procedimiento { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string VeterinarioNombre { get; set; } = string.Empty;

    public DateTime FechaProgramada { get; set; } = DateTime.Now;

    [Required, StringLength(20)]
    public string Estado { get; set; } = "Programada";

    [StringLength(800)]
    public string? Notas { get; set; }

    public Mascota? Mascota { get; set; }
}
