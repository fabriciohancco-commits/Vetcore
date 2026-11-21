using System.ComponentModel.DataAnnotations;

namespace Vetcare.API.DTOs;

public sealed class CitaRequestDto
{
    [Range(1, int.MaxValue)] public int PropietarioId { get; init; }
    [Range(1, int.MaxValue)] public int MascotaId { get; init; }
    [Required, StringLength(150, MinimumLength = 2)] public string VeterinarioNombre { get; init; } = string.Empty;
    public DateTime FechaHora { get; init; }
    [Range(15, 240)] public int DuracionMinutos { get; init; } = 30;
    [Required, StringLength(300, MinimumLength = 3)] public string Motivo { get; init; } = string.Empty;
}

public sealed class UpdateEstadoCitaDto
{
    [Required, StringLength(20)] public string Estado { get; init; } = string.Empty;
}

public sealed record CitaDto(
    int Id, int PropietarioId, string PropietarioNombre, int MascotaId, string MascotaNombre,
    string VeterinarioNombre, DateTime FechaHora, int DuracionMinutos, string Motivo, string Estado);
