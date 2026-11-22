using MediatR;
using Vetcare.Application.Common;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.Application.Mascotas;

public sealed record ListMascotasQuery(string? Search = null) : IRequest<List<Mascota>>;
public sealed record CreateMascotaCommand(Mascota Mascota) : IRequest<OperationResult<Mascota>>;
public sealed record UpdateMascotaCommand(int Id, Mascota Mascota) : IRequest<OperationResult<Mascota>>;
public sealed record DeleteMascotaCommand(int Id) : IRequest<OperationResult<bool>>;

internal sealed class ListMascotasHandler(IMascotaRepository mascotas)
    : IRequestHandler<ListMascotasQuery, List<Mascota>>
{
    public Task<List<Mascota>> Handle(ListMascotasQuery request, CancellationToken cancellationToken) =>
        mascotas.ListAsync(request.Search, cancellationToken);
}

internal sealed class CreateMascotaHandler(
    IMascotaRepository mascotas,
    IPropietarioRepository propietarios,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateMascotaCommand, OperationResult<Mascota>>
{
    public async Task<OperationResult<Mascota>> Handle(
        CreateMascotaCommand request,
        CancellationToken cancellationToken)
    {
        var pet = request.Mascota;
        var errors = ModelValidation.Validate(pet);
        if (pet.FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
            ModelValidation.AddError(errors, nameof(pet.FechaNacimiento), "La fecha de nacimiento no puede ser futura.");
        if (!await propietarios.ExistsAsync(pet.PropietarioId, cancellationToken))
            ModelValidation.AddError(errors, nameof(pet.PropietarioId), "El propietario seleccionado no existe.");
        if (errors.Count > 0) return new(400, Errors: errors);

        mascotas.Add(pet);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(201, pet);
    }
}

internal sealed class UpdateMascotaHandler(
    IMascotaRepository mascotas,
    IPropietarioRepository propietarios,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMascotaCommand, OperationResult<Mascota>>
{
    public async Task<OperationResult<Mascota>> Handle(
        UpdateMascotaCommand request,
        CancellationToken cancellationToken)
    {
        var current = await mascotas.GetByIdAsync(request.Id, cancellationToken);
        if (current is null) return new(404);

        var input = request.Mascota;
        var errors = ModelValidation.Validate(input);
        if (input.FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
            ModelValidation.AddError(errors, nameof(input.FechaNacimiento), "La fecha de nacimiento no puede ser futura.");
        if (!await propietarios.ExistsAsync(input.PropietarioId, cancellationToken))
            ModelValidation.AddError(errors, nameof(input.PropietarioId), "El propietario seleccionado no existe.");
        if (errors.Count > 0) return new(400, Errors: errors);

        current.Nombre = input.Nombre;
        current.Especie = input.Especie;
        current.Raza = input.Raza;
        current.FechaNacimiento = input.FechaNacimiento;
        current.PesoKg = input.PesoKg;
        current.Observaciones = input.Observaciones;
        current.PropietarioId = input.PropietarioId;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(200, current);
    }
}

internal sealed class DeleteMascotaHandler(
    IMascotaRepository mascotas,
    IConsultaMedicaRepository consultas,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteMascotaCommand, OperationResult<bool>>
{
    public async Task<OperationResult<bool>> Handle(
        DeleteMascotaCommand request,
        CancellationToken cancellationToken)
    {
        var pet = await mascotas.GetByIdAsync(request.Id, cancellationToken);
        if (pet is null) return new(404);
        if (await consultas.ExistsForMascotaAsync(request.Id, cancellationToken))
            return new(409, Message: "No se puede eliminar una mascota con consultas médicas registradas.");

        mascotas.Remove(pet);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(204, true);
    }
}
