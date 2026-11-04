using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetcare.Application.Common;
using Vetcare.Domain.Entities;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Application.Propietarios;

public sealed record  ListPropietariosQuery : IRequest<List<Propietario>>;
public sealed record CreatePropietarioCommand(Propietario Propietario) : IRequest<OperationResult<Propietario>>;
public sealed record UpdatePropietarioCommand(int Id, Propietario Propietario) : IRequest<OperationResult<Propietario>>;
public sealed record DeletePropietarioCommand(int Id) : IRequest<OperationResult<bool>>;

internal sealed class ListPropietariosHandler(VetCareDbContext db) : IRequestHandler<ListPropietariosQuery, List<Propietario>>
{
    public Task<List<Propietario>> Handle(ListPropietariosQuery request, CancellationToken cancellationToken) =>
        db.Propietarios.AsNoTracking().OrderBy(p => p.Apellidos).ThenBy(p => p.Nombres).ToListAsync(cancellationToken);
}

internal sealed class CreatePropietarioHandler(VetCareDbContext db) : IRequestHandler<CreatePropietarioCommand, OperationResult<Propietario>>
{
    public async Task<OperationResult<Propietario>> Handle(CreatePropietarioCommand request, CancellationToken cancellationToken)
    {
        var errors = ModelValidation.Validate(request.Propietario);
        if (errors.Count > 0) return new(400, Errors: errors);
        db.Propietarios.Add(request.Propietario);
        await db.SaveChangesAsync(cancellationToken);
        return new(201, request.Propietario);
    }
}

internal sealed class UpdatePropietarioHandler(VetCareDbContext db) : IRequestHandler<UpdatePropietarioCommand, OperationResult<Propietario>>
{
    public async Task<OperationResult<Propietario>> Handle(UpdatePropietarioCommand request, CancellationToken cancellationToken)
    {
        var actual = await db.Propietarios.FindAsync([request.Id], cancellationToken);
        if (actual is null) return new(404);
        request.Propietario.Id = request.Id;
        var errors = ModelValidation.Validate(request.Propietario);
        if (errors.Count > 0) return new(400, Errors: errors);
        actual.Nombres = request.Propietario.Nombres;
        actual.Apellidos = request.Propietario.Apellidos;
        actual.Telefono = request.Propietario.Telefono;
        actual.Correo = request.Propietario.Correo;
        actual.Direccion = request.Propietario.Direccion;
        await db.SaveChangesAsync(cancellationToken);
        return new(200, actual);
    }
}

internal sealed class DeletePropietarioHandler(VetCareDbContext db) : IRequestHandler<DeletePropietarioCommand, OperationResult<bool>>
{
    public async Task<OperationResult<bool>> Handle(DeletePropietarioCommand request, CancellationToken cancellationToken)
    {
        var propietario = await db.Propietarios.FindAsync([request.Id], cancellationToken);
        if (propietario is null) return new(404);
        if (await db.Mascotas.AnyAsync(m => m.PropietarioId == request.Id, cancellationToken))
            return new(409, Message: "El propietario tiene mascotas registradas. Reasígnelas o elimínelas antes de continuar.");
        db.Propietarios.Remove(propietario);
        await db.SaveChangesAsync(cancellationToken);
        return new(204, true);
    }
}
