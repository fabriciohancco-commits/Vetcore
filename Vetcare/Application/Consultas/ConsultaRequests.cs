using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetcare.Application.Common;
using Vetcare.Domain.Entities;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Application.Consultas;

public sealed record ListConsultasQuery : IRequest<List<ConsultaMedica>>;
public sealed record GetConsultaQuery(int Id) : IRequest<ConsultaMedica?>;
public sealed record GetHistorialMascotaQuery(int MascotaId) : IRequest<HistorialMascotaResult?>;
public sealed record HistorialMascotaResult(List<ConsultaMedica> Consultas);
public sealed record CreateConsultaCommand(ConsultaMedica Consulta) : IRequest<OperationResult<ConsultaMedica>>;
public sealed record UpdateConsultaCommand(int Id, ConsultaMedica Consulta) : IRequest<OperationResult<ConsultaMedica>>;
public sealed record DeleteConsultaCommand(int Id) : IRequest<OperationResult<bool>>;

internal sealed class ListConsultasHandler(VetCareDbContext db) : IRequestHandler<ListConsultasQuery, List<ConsultaMedica>>
{
    public Task<List<ConsultaMedica>> Handle(ListConsultasQuery request, CancellationToken cancellationToken) =>
        db.ConsultasMedicas.AsNoTracking().Include(c => c.Mascota)
            .OrderByDescending(c => c.FechaConsulta).ToListAsync(cancellationToken);
}

internal sealed class GetConsultaHandler(VetCareDbContext db) : IRequestHandler<GetConsultaQuery, ConsultaMedica?>
{
    public Task<ConsultaMedica?> Handle(GetConsultaQuery request, CancellationToken cancellationToken) =>
        db.ConsultasMedicas.AsNoTracking().Include(c => c.Mascota)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
}

internal sealed class GetHistorialMascotaHandler(VetCareDbContext db) : IRequestHandler<GetHistorialMascotaQuery, HistorialMascotaResult?>
{
    public async Task<HistorialMascotaResult?> Handle(GetHistorialMascotaQuery request, CancellationToken cancellationToken)
    {
        var existe = await db.Mascotas.AnyAsync(m => m.Id == request.MascotaId, cancellationToken);
        if (!existe) return null;
        var consultas = await db.ConsultasMedicas.AsNoTracking()
            .Where(c => c.MascotaId == request.MascotaId)
            .OrderByDescending(c => c.FechaConsulta).ToListAsync(cancellationToken);
        return new(consultas);
    }
}

internal sealed class CreateConsultaHandler(VetCareDbContext db) : IRequestHandler<CreateConsultaCommand, OperationResult<ConsultaMedica>>
{
    public async Task<OperationResult<ConsultaMedica>> Handle(CreateConsultaCommand request, CancellationToken cancellationToken)
    {
        var consulta = request.Consulta;
        var errors = ModelValidation.Validate(consulta);
        if (consulta.FechaConsulta > DateTime.Now)
            ModelValidation.AddError(errors, nameof(consulta.FechaConsulta), "La fecha de consulta no puede ser futura.");
        if (!await db.Mascotas.AnyAsync(m => m.Id == consulta.MascotaId, cancellationToken))
            ModelValidation.AddError(errors, nameof(consulta.MascotaId), "La mascota seleccionada no existe.");
        if (errors.Count > 0) return new(400, Errors: errors);
        db.ConsultasMedicas.Add(consulta);
        await db.SaveChangesAsync(cancellationToken);
        return new(201, consulta);
    }
}

internal sealed class UpdateConsultaHandler(VetCareDbContext db) : IRequestHandler<UpdateConsultaCommand, OperationResult<ConsultaMedica>>
{
    public async Task<OperationResult<ConsultaMedica>> Handle(UpdateConsultaCommand request, CancellationToken cancellationToken)
    {
        var actual = await db.ConsultasMedicas.FindAsync([request.Id], cancellationToken);
        if (actual is null) return new(404);
        var input = request.Consulta;
        var errors = ModelValidation.Validate(input);
        if (input.FechaConsulta > DateTime.Now)
            ModelValidation.AddError(errors, nameof(input.FechaConsulta), "La fecha de consulta no puede ser futura.");
        if (!await db.Mascotas.AnyAsync(m => m.Id == input.MascotaId, cancellationToken))
            ModelValidation.AddError(errors, nameof(input.MascotaId), "La mascota seleccionada no existe.");
        if (errors.Count > 0) return new(400, Errors: errors);
        actual.MascotaId = input.MascotaId;
        actual.FechaConsulta = input.FechaConsulta;
        actual.VeterinarioNombre = input.VeterinarioNombre;
        actual.Motivo = input.Motivo;
        actual.Diagnostico = input.Diagnostico;
        actual.Tratamiento = input.Tratamiento;
        actual.VacunasAplicadas = input.VacunasAplicadas;
        await db.SaveChangesAsync(cancellationToken);
        return new(200, actual);
    }
}

internal sealed class DeleteConsultaHandler(VetCareDbContext db) : IRequestHandler<DeleteConsultaCommand, OperationResult<bool>>
{
    public async Task<OperationResult<bool>> Handle(DeleteConsultaCommand request, CancellationToken cancellationToken)
    {
        var consulta = await db.ConsultasMedicas.FindAsync([request.Id], cancellationToken);
        if (consulta is null) return new(404);
        db.ConsultasMedicas.Remove(consulta);
        await db.SaveChangesAsync(cancellationToken);
        return new(204, true);
    }
}
