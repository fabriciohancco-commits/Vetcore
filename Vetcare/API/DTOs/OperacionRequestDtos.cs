using System.ComponentModel.DataAnnotations;

namespace Vetcare.API.DTOs;

public sealed class OperacionRequestDto
{
    [Range(1, int.MaxValue)] public int MascotaId { get; init; }
    [Required, StringLength(140, MinimumLength = 3)] public string Procedimiento { get; init; } = string.Empty;
    [Required, StringLength(150)] public string VeterinarioNombre { get; init; } = string.Empty;
    public DateTime FechaProgramada { get; init; }
    [StringLength(20)] public string Estado { get; init; } = "Programada";
    [StringLength(800)] public string? Notas { get; init; }
}

public sealed class UpdateEstadoOperacionDto
{
    [Required, StringLength(20)] public string Estado { get; init; } = string.Empty;
}

public sealed record OperacionDto(
    int Id, int MascotaId, string MascotaNombre, string Procedimiento,
    string VeterinarioNombre, DateTime FechaProgramada, string Estado, string? Notas);
