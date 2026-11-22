using MediatR;
using Vetcare.Application.Common;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.Application.UseCases.Propietarios.Commands;

public sealed record CreatePropietarioCommand(Propietario Propietario) : IRequest<OperationResult<Propietario>>;
public sealed record UpdatePropietarioCommand(int Id, Propietario Propietario) : IRequest<OperationResult<Propietario>>;
public sealed record DeletePropietarioCommand(int Id) : IRequest<OperationResult<bool>>;

internal sealed class CreatePropietarioHandler(
    IPropietarioRepository propietarios,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePropietarioCommand, OperationResult<Propietario>>
{
    public async Task<OperationResult<Propietario>> Handle(
        CreatePropietarioCommand request,
        CancellationToken cancellationToken)
    {
        var errors = ModelValidation.Validate(request.Propietario);
        if (errors.Count > 0) return new(400, Errors: errors);

        propietarios.Add(request.Propietario);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(201, request.Propietario);
    }
}

internal sealed class UpdatePropietarioHandler(
    IPropietarioRepository propietarios,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePropietarioCommand, OperationResult<Propietario>>
{
    public async Task<OperationResult<Propietario>> Handle(
        UpdatePropietarioCommand request,
        CancellationToken cancellationToken)
    {
        var current = await propietarios.GetByIdAsync(request.Id, cancellationToken);
        if (current is null) return new(404);

        request.Propietario.Id = request.Id;
        var errors = ModelValidation.Validate(request.Propietario);
        if (errors.Count > 0) return new(400, Errors: errors);

        current.Nombres = request.Propietario.Nombres;
        current.Apellidos = request.Propietario.Apellidos;
        current.Telefono = request.Propietario.Telefono;
        current.Correo = request.Propietario.Correo;
        current.Direccion = request.Propietario.Direccion;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(200, current);
    }
}

internal sealed class DeletePropietarioHandler(
    IPropietarioRepository propietarios,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePropietarioCommand, OperationResult<bool>>
{
    public async Task<OperationResult<bool>> Handle(
        DeletePropietarioCommand request,
        CancellationToken cancellationToken)
    {
        var owner = await propietarios.GetByIdAsync(request.Id, cancellationToken);
        if (owner is null) return new(404);
        if (await propietarios.HasMascotasAsync(request.Id, cancellationToken))
            return new(409, Message: "El propietario tiene mascotas registradas. Reasígnelas o elimínelas antes de continuar.");

        propietarios.Remove(owner);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(204, true);
    }
}
