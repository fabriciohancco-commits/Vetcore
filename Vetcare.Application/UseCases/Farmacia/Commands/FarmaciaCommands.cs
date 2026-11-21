using MediatR;
using Vetcare.Application.Common;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.Application.UseCases.Farmacia.Commands;

public sealed record CreateMedicamentoCommand(Medicamento Medicamento) : IRequest<OperationResult<Medicamento>>;
public sealed record UpdateMedicamentoCommand(int Id, Medicamento Medicamento) : IRequest<OperationResult<Medicamento>>;
public sealed record CreatePrescripcionCommand(Prescripcion Prescripcion) : IRequest<OperationResult<Prescripcion>>;

internal sealed class CreateMedicamentoHandler(IMedicamentoRepository medicamentos, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateMedicamentoCommand, OperationResult<Medicamento>>
{
    public async Task<OperationResult<Medicamento>> Handle(CreateMedicamentoCommand request, CancellationToken cancellationToken)
    {
        var value = request.Medicamento;
        var errors = ModelValidation.Validate(value);
        if (value.FechaVencimiento is not null && value.FechaVencimiento.Value.Date < DateTime.Today)
            ModelValidation.AddError(errors, nameof(value.FechaVencimiento), "No se puede registrar un medicamento vencido.");
        if (errors.Count > 0) return new(400, Errors: errors);
        medicamentos.Add(value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(201, value);
    }
}

internal sealed class UpdateMedicamentoHandler(IMedicamentoRepository medicamentos, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMedicamentoCommand, OperationResult<Medicamento>>
{
    public async Task<OperationResult<Medicamento>> Handle(UpdateMedicamentoCommand request, CancellationToken cancellationToken)
    {
        var current = await medicamentos.GetByIdAsync(request.Id, cancellationToken);
        if (current is null) return new(404);
        var input = request.Medicamento;
        var errors = ModelValidation.Validate(input);
        if (input.FechaVencimiento is not null && input.FechaVencimiento.Value.Date < DateTime.Today)
            ModelValidation.AddError(errors, nameof(input.FechaVencimiento), "No se puede registrar un medicamento vencido.");
        if (errors.Count > 0) return new(400, Errors: errors);
        current.Nombre = input.Nombre;
        current.PrincipioActivo = input.PrincipioActivo;
        current.Presentacion = input.Presentacion;
        current.RequiereReceta = input.RequiereReceta;
        current.StockUnidades = input.StockUnidades;
        current.StockMinimo = input.StockMinimo;
        current.FechaVencimiento = input.FechaVencimiento;
        current.Activo = input.Activo;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(200, current);
    }
}

internal sealed class CreatePrescripcionHandler(
    IPrescripcionRepository prescripciones,
    IMedicamentoRepository medicamentos,
    IMascotaRepository mascotas,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePrescripcionCommand, OperationResult<Prescripcion>>
{
    public async Task<OperationResult<Prescripcion>> Handle(CreatePrescripcionCommand request, CancellationToken cancellationToken)
    {
        var value = request.Prescripcion;
        var errors = ModelValidation.Validate(value);
        var medicine = await medicamentos.GetByIdAsync(value.MedicamentoId, cancellationToken);
        if (medicine is null)
            ModelValidation.AddError(errors, nameof(value.MedicamentoId), "Selecciona un medicamento vigente del catálogo.");
        else if (!medicine.Activo)
            ModelValidation.AddError(errors, nameof(value.MedicamentoId), "El medicamento está inactivo.");
        else if (medicine.StockUnidades < value.UnidadesEntregadas)
            ModelValidation.AddError(errors, nameof(value.UnidadesEntregadas), "No hay existencias suficientes para entregar esa cantidad.");
        if (!await mascotas.ExistsAsync(value.MascotaId, cancellationToken))
            ModelValidation.AddError(errors, nameof(value.MascotaId), "La mascota seleccionada no existe.");
        if (medicine?.RequiereReceta == true && string.IsNullOrWhiteSpace(value.NumeroReceta))
            ModelValidation.AddError(errors, nameof(value.NumeroReceta), "Este medicamento exige registrar el número de receta veterinaria.");
        if (errors.Count > 0) return new(400, Errors: errors);
        value.FechaRegistro = DateTime.Now;
        prescripciones.Add(value);
        medicine!.StockUnidades -= value.UnidadesEntregadas;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(201, value);
    }
}
