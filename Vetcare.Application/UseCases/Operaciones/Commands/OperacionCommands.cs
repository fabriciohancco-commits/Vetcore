using MediatR;
using Vetcare.Application.Common;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.Application.UseCases.Operaciones.Commands;

public sealed record CreateOperacionCommand(Operacion Operacion) : IRequest<OperationResult<Operacion>>;
public sealed record UpdateEstadoOperacionCommand(int Id, string Estado) : IRequest<OperationResult<Operacion>>;
public sealed record DeleteOperacionCommand(int Id) : IRequest<OperationResult<bool>>;

internal sealed class CreateOperacionHandler(
    IOperacionRepository operaciones,
    IMascotaRepository mascotas,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOperacionCommand, OperationResult<Operacion>>
{
    private static readonly string[] Estados = ["Programada", "En curso", "Completada", "Cancelada"];

    public async Task<OperationResult<Operacion>> Handle(CreateOperacionCommand request, CancellationToken cancellationToken)
    {
        var value = request.Operacion;
        var errors = ModelValidation.Validate(value);
        if (!Estados.Contains(value.Estado))
            ModelValidation.AddError(errors, nameof(value.Estado), "Selecciona un estado válido.");
        if (!await mascotas.ExistsAsync(value.MascotaId, cancellationToken))
            ModelValidation.AddError(errors, nameof(value.MascotaId), "La mascota seleccionada no existe.");
        if (errors.Count > 0) return new(400, Errors: errors);
        operaciones.Add(value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(201, value);
    }
}

internal sealed class UpdateEstadoOperacionHandler(IOperacionRepository operaciones, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateEstadoOperacionCommand, OperationResult<Operacion>>
{
    private static readonly string[] Estados = ["Programada", "En curso", "Completada", "Cancelada"];

    public async Task<OperationResult<Operacion>> Handle(UpdateEstadoOperacionCommand request, CancellationToken cancellationToken)
    {
        var current = await operaciones.GetByIdAsync(request.Id, cancellationToken);
        if (current is null) return new(404);
        if (!Estados.Contains(request.Estado))
            return new(400, Errors: new Dictionary<string, string[]> { [nameof(request.Estado)] = ["Selecciona un estado válido."] });
        current.Estado = request.Estado;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(200, current);
    }
}

internal sealed class DeleteOperacionHandler(IOperacionRepository operaciones, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteOperacionCommand, OperationResult<bool>>
{
    public async Task<OperationResult<bool>> Handle(DeleteOperacionCommand request, CancellationToken cancellationToken)
    {
        var current = await operaciones.GetByIdAsync(request.Id, cancellationToken);
        if (current is null) return new(404);
        operaciones.Remove(current);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(204, true);
    }
}
