using System.ComponentModel.DataAnnotations;

namespace Vetcare.API.DTOs;

public sealed class MedicamentoRequestDto
{
    [Required, StringLength(120, MinimumLength = 2)] public string Nombre { get; init; } = string.Empty;
    [StringLength(120)] public string? PrincipioActivo { get; init; }
    [Required, StringLength(80)] public string Presentacion { get; init; } = string.Empty;
    public bool RequiereReceta { get; init; }
    [Range(0, 100000)] public int StockUnidades { get; init; }
    [Range(0, 100000)] public int StockMinimo { get; init; }
    public DateTime? FechaVencimiento { get; init; }
}

public sealed class PrescripcionRequestDto
{
    [Range(1, int.MaxValue)] public int MascotaId { get; init; }
    [Range(1, int.MaxValue)] public int MedicamentoId { get; init; }
    [Required, StringLength(150)] public string VeterinarioNombre { get; init; } = string.Empty;
    [StringLength(60)] public string? NumeroReceta { get; init; }
    [Required, StringLength(1000, MinimumLength = 3)] public string Indicaciones { get; init; } = string.Empty;
    [StringLength(200)] public string? Notas { get; init; }
}

public sealed record MedicamentoDto(
    int Id, string Nombre, string? PrincipioActivo, string Presentacion,
    bool RequiereReceta, int StockUnidades, int StockMinimo, DateTime? FechaVencimiento, bool Activo);

public sealed record PrescripcionDto(
    int Id, int MascotaId, string MascotaNombre, int MedicamentoId, string MedicamentoNombre,
    bool RequiereReceta, DateTime FechaRegistro, string VeterinarioNombre,
    string? NumeroReceta, string Indicaciones, string? Notas);
