using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetcare.Application.Common;
using Vetcare.Domain.Entities;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Application.Mascotas;

public sealed record ListMascotasQuery : IRequest<List<Mascota>>;
public sealed record CreateMascotaCommand(Mascota Mascota) : IRequest<OperationResult<Mascota>>;
public sealed record UpdateMascotaCommand(int Id, Mascota Mascota) : IRequest<OperationResult<Mascota>>;
public sealed record DeleteMascotaCommand(int Id) : IRequest<OperationResult<bool>>;

internal sealed class ListMascotasHandler(VetCareDbContext db) : IRequestHandler<ListMascotasQuery, List<Mascota>>
{
    public Task<List<Mascota>> Handle(ListMascotasQuery request, CancellationToken cancellationToken) =>
        db.Mascotas.AsNoTracking().OrderBy(m => m.Nombre).ToListAsync(cancellationToken);
}

internal sealed class CreateMascotaHandler(VetCareDbContext db) : IRequestHandler<CreateMascotaCommand, OperationResult<Mascota>>
{
    public async Task<OperationResult<Mascota>> Handle(CreateMascotaCommand request, CancellationToken cancellationToken)
    {
        var mascota = request.Mascota;
        var errors = ModelValidation.Validate(mascota);
        if (mascota.FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
            ModelValidation.AddError(errors, nameof(mascota.FechaNacimiento), "La fecha de nacimiento no puede ser futura.");
        if (!await db.Propietarios.AnyAsync(p => p.Id == mascota.PropietarioId, cancellationToken))
            ModelValidation.AddError(errors, nameof(mascota.PropietarioId), "El propietario seleccionado no existe.");
        if (errors.Count > 0) return new(400, Errors: errors);
        db.Mascotas.Add(mascota);
        await db.SaveChangesAsync(cancellationToken);
        return new(201, mascota);
    }
}

internal sealed class UpdateMascotaHandler(VetCareDbContext db) : IRequestHandler<UpdateMascotaCommand, OperationResult<Mascota>>
{
    public async Task<OperationResult<Mascota>> Handle(UpdateMascotaCommand request, CancellationToken cancellationToken)
    {
        var actual = await db.Mascotas.FindAsync([request.Id], cancellationToken);
        if (actual is null) return new(404);
        var input = request.Mascota;
        var errors = ModelValidation.Validate(input);
        if (input.FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
            ModelValidation.AddError(errors, nameof(input.FechaNacimiento), "La fecha de nacimiento no puede ser futura.");
        if (!await db.Propietarios.AnyAsync(p => p.Id == input.PropietarioId, cancellationToken))
            ModelValidation.AddError(errors, nameof(input.PropietarioId), "El propietario seleccionado no existe.");
        if (errors.Count > 0) return new(400, Errors: errors);
        actual.Nombre = input.Nombre;
        actual.Especie = input.Especie;
        actual.Raza = input.Raza;
        actual.FechaNacimiento = input.FechaNacimiento;
        actual.PesoKg = input.PesoKg;
        actual.Observaciones = input.Observaciones;
        actual.PropietarioId = input.PropietarioId;
        await db.SaveChangesAsync(cancellationToken);
        return new(200, actual);
    }
}

internal sealed class DeleteMascotaHandler(VetCareDbContext db) : IRequestHandler<DeleteMascotaCommand, OperationResult<bool>>
{
    public async Task<OperationResult<bool>> Handle(DeleteMascotaCommand request, CancellationToken cancellationToken)
    {
        var mascota = await db.Mascotas.FindAsync([request.Id], cancellationToken);
        if (mascota is null) return new(404);
        db.Mascotas.Remove(mascota);
        await db.SaveChangesAsync(cancellationToken);
        return new(204, true);
    }
}
