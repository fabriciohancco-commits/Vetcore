using Vetcare.Domain.Entities;

namespace Vetcare.API.DTOs;

internal static class DtoMappings
{
    public static Propietario ToEntity(this PropietarioRequestDto input) => new()
    {
        Nombres = input.Nombres,
        Apellidos = input.Apellidos,
        Telefono = input.Telefono,
        Correo = input.Correo,
        Direccion = input.Direccion
    };

    public static PropietarioDto ToDto(this Propietario value) =>
        new(value.Id, value.Nombres, value.Apellidos, value.Telefono, value.Correo, value.Direccion);

    public static Mascota ToEntity(this MascotaRequestDto input) => new()
    {
        Nombre = input.Nombre,
        Especie = input.Especie,
        Raza = input.Raza,
        FechaNacimiento = input.FechaNacimiento,
        PesoKg = input.PesoKg,
        Observaciones = input.Observaciones,
        PropietarioId = input.PropietarioId
    };

    public static MascotaDto ToDto(this Mascota value) =>
        new(value.Id, value.Nombre, value.Especie, value.Raza, value.FechaNacimiento,
            value.PesoKg, value.Observaciones, value.PropietarioId);

    public static ConsultaMedica ToEntity(this ConsultaMedicaRequestDto input) => new()
    {
        MascotaId = input.MascotaId,
        FechaConsulta = input.FechaConsulta,
        VeterinarioNombre = input.VeterinarioNombre,
        Motivo = input.Motivo,
        Diagnostico = input.Diagnostico,
        Tratamiento = input.Tratamiento,
        VacunasAplicadas = input.VacunasAplicadas
    };

    public static ConsultaMedicaDto ToDto(this ConsultaMedica value) =>
        new(value.Id, value.MascotaId, value.FechaConsulta, value.VeterinarioNombre,
            value.Motivo, value.Diagnostico, value.Tratamiento, value.VacunasAplicadas,
            value.Mascota?.ToDto());

    public static Medicamento ToEntity(this MedicamentoRequestDto input) => new()
    {
        Nombre = input.Nombre,
        PrincipioActivo = input.PrincipioActivo,
        Presentacion = input.Presentacion,
        RequiereReceta = input.RequiereReceta,
        StockUnidades = input.StockUnidades,
        StockMinimo = input.StockMinimo,
        FechaVencimiento = input.FechaVencimiento
    };

    public static MedicamentoDto ToDto(this Medicamento value) => new(
        value.Id, value.Nombre, value.PrincipioActivo, value.Presentacion, value.RequiereReceta,
        value.StockUnidades, value.StockMinimo, value.FechaVencimiento, value.Activo);

    public static Prescripcion ToEntity(this PrescripcionRequestDto input) => new()
    {
        MascotaId = input.MascotaId,
        MedicamentoId = input.MedicamentoId,
        VeterinarioNombre = input.VeterinarioNombre,
        NumeroReceta = input.NumeroReceta,
        Dosis = input.Dosis,
        Frecuencia = input.Frecuencia,
        Duracion = input.Duracion,
        Indicaciones = input.Indicaciones,
        Notas = input.Notas
    };

    public static PrescripcionDto ToDto(this Prescripcion value) => new(
        value.Id, value.MascotaId, value.Mascota?.Nombre ?? "Mascota", value.MedicamentoId,
        value.Medicamento?.Nombre ?? "Medicamento", value.Medicamento?.RequiereReceta ?? false,
        value.FechaRegistro, value.VeterinarioNombre, value.NumeroReceta, value.Dosis,
        value.Frecuencia, value.Duracion, value.Indicaciones, value.Notas);

    public static Operacion ToEntity(this OperacionRequestDto input) => new()
    {
        MascotaId = input.MascotaId,
        Procedimiento = input.Procedimiento,
        VeterinarioNombre = input.VeterinarioNombre,
        FechaProgramada = input.FechaProgramada,
        Estado = input.Estado,
        Notas = input.Notas
    };

    public static OperacionDto ToDto(this Operacion value) => new(
        value.Id, value.MascotaId, value.Mascota?.Nombre ?? "Mascota", value.Procedimiento,
        value.VeterinarioNombre, value.FechaProgramada, value.Estado, value.Notas);
}
