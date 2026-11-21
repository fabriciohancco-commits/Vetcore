using MediatR;
using Vetcare.Application.Common;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.Application.UseCases.Citas.Commands;

public sealed record CreateCitaCommand(Cita Cita) : IRequest<OperationResult<Cita>>;
public sealed record UpdateEstadoCitaCommand(int Id, string Estado) : IRequest<OperationResult<Cita>>;

internal sealed class CreateCitaHandler(
    ICitaRepository citas,
    IPropietarioRepository propietarios,
    IMascotaRepository mascotas,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCitaCommand, OperationResult<Cita>>
{
    private static readonly string[] Estados = ["Programada", "Atendida", "No asistió", "Cancelada"];

    public async Task<OperationResult<Cita>> Handle(CreateCitaCommand request, CancellationToken cancellationToken)
    {
        var value = request.Cita;
        value.VeterinarioNombre = value.VeterinarioNombre.Trim();
        var errors = ModelValidation.Validate(value);
        if (!Estados.Contains(value.Estado))
            ModelValidation.AddError(errors, nameof(value.Estado), "Selecciona un estado válido.");
        if (value.FechaHora <= DateTime.Now)
            ModelValidation.AddError(errors, nameof(value.FechaHora), "La cita debe programarse para una fecha futura.");

        var pet = await mascotas.GetByIdAsync(value.MascotaId, cancellationToken);
        if (pet is null)
            ModelValidation.AddError(errors, nameof(value.MascotaId), "La mascota seleccionada no existe.");
        else if (pet.PropietarioId != value.PropietarioId)
            ModelValidation.AddError(errors, nameof(value.PropietarioId), "La mascota no pertenece al dueño seleccionado.");
        if (!await propietarios.ExistsAsync(value.PropietarioId, cancellationToken))
            ModelValidation.AddError(errors, nameof(value.PropietarioId), "El dueño seleccionado no existe.");
        if (errors.Count > 0) return new(400, Errors: errors);

        var start = value.FechaHora;
        var end = start.AddMinutes(value.DuracionMinutos);
        var possibleConflicts = await citas.ListByVeterinarioAndPeriodAsync(
            value.VeterinarioNombre, start.AddMinutes(-240), end, cancellationToken);
        if (possibleConflicts.Any(existing =>
                existing.FechaHora < end
                && existing.FechaHora.AddMinutes(existing.DuracionMinutos) > start))
        {
            return new(409, Message: "Ese veterinario ya tiene una cita que se cruza con el horario seleccionado.");
        }

        citas.Add(value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(201, value);
    }
}

internal sealed class UpdateEstadoCitaHandler(ICitaRepository citas, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateEstadoCitaCommand, OperationResult<Cita>>
{
    private static readonly string[] Estados = ["Programada", "Atendida", "No asistió", "Cancelada"];

    public async Task<OperationResult<Cita>> Handle(UpdateEstadoCitaCommand request, CancellationToken cancellationToken)
    {
        var current = await citas.GetByIdAsync(request.Id, cancellationToken);
        if (current is null) return new(404);
        if (!Estados.Contains(request.Estado))
            return new(400, Errors: new Dictionary<string, string[]> { [nameof(request.Estado)] = ["Selecciona un estado válido."] });
        current.Estado = request.Estado;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(200, current);
    }
}
