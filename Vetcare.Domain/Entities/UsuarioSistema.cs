using System.ComponentModel.DataAnnotations;

namespace Vetcare.Domain.Entities;

public sealed class UsuarioSistema
{
    public int Id { get; set; }

    [Required, EmailAddress, StringLength(160)]
    public string Correo { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Rol { get; set; } = "Veterinario";

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
