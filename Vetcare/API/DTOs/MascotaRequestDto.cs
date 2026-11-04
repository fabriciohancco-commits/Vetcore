using System.ComponentModel.DataAnnotations;

namespace Vetcare.API.DTOs;

public sealed class MascotaRequestDto
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string Nombre { get; init; } = string.Empty;

    [Required, StringLength(40)]
    public string Especie { get; init; } = string.Empty;

    [StringLength(60)]
    public string? Raza { get; init; }

    public DateOnly? FechaNacimiento { get; init; }

    [Range(0.01, 500)]
    public decimal? PesoKg { get; init; }

    [StringLength(500)]
    public string? Observaciones { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un propietario.")]
    public int PropietarioId { get; init; }
}

public sealed record MascotaDto(
    int Id,
    string Nombre,
    string Especie,
    string? Raza,
    DateOnly? FechaNacimiento,
    decimal? PesoKg,
    string? Observaciones,
    int PropietarioId);
