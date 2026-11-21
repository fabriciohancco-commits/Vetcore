using System.ComponentModel.DataAnnotations;

namespace Vetcare.Domain.Entities;

public sealed class Cita
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int PropietarioId { get; set; }

    [Range(1, int.MaxValue)]
    public int MascotaId { get; set; }

    [Required, StringLength(150, MinimumLength = 2)]
    public string VeterinarioNombre { get; set; } = string.Empty;

    public DateTime FechaHora { get; set; }

    [Range(15, 240)]
    public int DuracionMinutos { get; set; } = 30;

    [Required, StringLength(300, MinimumLength = 3)]
    public string Motivo { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Estado { get; set; } = "Programada";

    public Propietario? Propietario { get; set; }
    public Mascota? Mascota { get; set; }
}
