using System.ComponentModel.DataAnnotations;

namespace Vetcare.API.DTOs;

public sealed class ConsultaMedicaRequestDto
{
    [Range(1, int.MaxValue)]
    public int MascotaId { get; init; }

    public DateTime FechaConsulta { get; init; } = DateTime.Now;

    [Required, StringLength(150)]
    public string VeterinarioNombre { get; init; } = string.Empty;

    [Required, StringLength(500)]
    public string Motivo { get; init; } = string.Empty;

    [Required, StringLength(1000)]
    public string Diagnostico { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Tratamiento { get; init; }

    [StringLength(1000)]
    public string? VacunasAplicadas { get; init; }
}

public sealed record ConsultaMedicaDto(
    int Id,
    int MascotaId,
    DateTime FechaConsulta,
    string VeterinarioNombre,
    string Motivo,
    string Diagnostico,
    string? Tratamiento,
    string? VacunasAplicadas,
    MascotaDto? Mascota);
