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
}
