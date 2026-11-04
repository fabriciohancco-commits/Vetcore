using System.ComponentModel.DataAnnotations;

namespace Vetcare.API.DTOs;

public sealed class PropietarioRequestDto
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string Nombres { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string Apellidos { get; init; } = string.Empty;

    [Required, Phone, StringLength(25)]
    public string Telefono { get; init; } = string.Empty;

    [EmailAddress, StringLength(120)]
    public string? Correo { get; init; }

    [StringLength(180)]
    public string? Direccion { get; init; }
}

public sealed record PropietarioDto(
    int Id,
    string Nombres,
    string Apellidos,
    string Telefono,
    string? Correo,
    string? Direccion);
