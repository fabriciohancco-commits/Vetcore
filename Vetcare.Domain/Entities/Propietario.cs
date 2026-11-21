using System.ComponentModel.DataAnnotations;

namespace Vetcare.Domain.Entities;

public sealed class Propietario
{
    public int Id { get; set; }

    [Required, StringLength(80, MinimumLength = 2)]
    public string Nombres { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string Apellidos { get; set; } = string.Empty;

    [Required, Phone, StringLength(25)]
    public string Telefono { get; set; } = string.Empty;

    [EmailAddress, StringLength(120)]
    public string? Correo { get; set; }

    [StringLength(180)]
    public string? Direccion { get; set; }
}
