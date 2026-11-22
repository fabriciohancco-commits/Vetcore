using MediatR;
using Vetcare.Application.Common;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.Application.UseCases.Consultas.Commands;

public sealed record CreateConsultaCommand(ConsultaMedica Consulta) : IRequest<OperationResult<ConsultaMedica>>;
public sealed record UpdateConsultaCommand(int Id, ConsultaMedica Consulta) : IRequest<OperationResult<ConsultaMedica>>;
public sealed record DeleteConsultaCommand(int Id) : IRequest<OperationResult<bool>>;

internal sealed class CreateConsultaHandler(
    IConsultaMedicaRepository consultas,
    IMascotaRepository mascotas,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateConsultaCommand, OperationResult<ConsultaMedica>>
{
    public async Task<OperationResult<ConsultaMedica>> Handle(
        CreateConsultaCommand request,
        CancellationToken cancellationToken)
    {
        var consultation = request.Consulta;
        var errors = ModelValidation.Validate(consultation);
        if (consultation.FechaConsulta > DateTime.Now)
            ModelValidation.AddError(errors, nameof(consultation.FechaConsulta), "La fecha de consulta no puede ser futura.");
        if (!await mascotas.ExistsAsync(consultation.MascotaId, cancellationToken))
            ModelValidation.AddError(errors, nameof(consultation.MascotaId), "La mascota seleccionada no existe.");
        if (errors.Count > 0) return new(400, Errors: errors);

        consultas.Add(consultation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(201, consultation);
    }
}

internal sealed class UpdateConsultaHandler(
    IConsultaMedicaRepository consultas,
    IMascotaRepository mascotas,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateConsultaCommand, OperationResult<ConsultaMedica>>
{
    public async Task<OperationResult<ConsultaMedica>> Handle(
        UpdateConsultaCommand request,
        CancellationToken cancellationToken)
    {
        var current = await consultas.GetByIdAsync(request.Id, cancellationToken);
        if (current is null) return new(404);

        var input = request.Consulta;
        var errors = ModelValidation.Validate(input);
        if (input.FechaConsulta > DateTime.Now)
            ModelValidation.AddError(errors, nameof(input.FechaConsulta), "La fecha de consulta no puede ser futura.");
        if (!await mascotas.ExistsAsync(input.MascotaId, cancellationToken))
            ModelValidation.AddError(errors, nameof(input.MascotaId), "La mascota seleccionada no existe.");
        if (errors.Count > 0) return new(400, Errors: errors);

        current.MascotaId = input.MascotaId;
        current.FechaConsulta = input.FechaConsulta;
        current.VeterinarioNombre = input.VeterinarioNombre;
        current.Motivo = input.Motivo;
        current.Diagnostico = input.Diagnostico;
        current.Tratamiento = input.Tratamiento;
        current.VacunasAplicadas = input.VacunasAplicadas;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(200, current);
    }
}

internal sealed class DeleteConsultaHandler(
    IConsultaMedicaRepository consultas,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteConsultaCommand, OperationResult<bool>>
{
    public async Task<OperationResult<bool>> Handle(
        DeleteConsultaCommand request,
        CancellationToken cancellationToken)
    {
        var consultation = await consultas.GetByIdAsync(request.Id, cancellationToken);
        if (consultation is null) return new(404);

        consultas.Remove(consultation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(204, true);
    }
}
