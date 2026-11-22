using MediatR;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.Application.UseCases.Consultas.Queries;

public sealed record ListConsultasQuery : IRequest<List<ConsultaMedica>>;
public sealed record GetConsultaQuery(int Id) : IRequest<ConsultaMedica?>;
public sealed record GetHistorialMascotaQuery(int MascotaId) : IRequest<HistorialMascotaResult?>;
public sealed record HistorialMascotaResult(List<ConsultaMedica> Consultas);

internal sealed class ListConsultasHandler(IConsultaMedicaRepository consultas)
    : IRequestHandler<ListConsultasQuery, List<ConsultaMedica>>
{
    public Task<List<ConsultaMedica>> Handle(ListConsultasQuery request, CancellationToken cancellationToken) =>
        consultas.ListAsync(cancellationToken);
}

internal sealed class GetConsultaHandler(IConsultaMedicaRepository consultas)
    : IRequestHandler<GetConsultaQuery, ConsultaMedica?>
{
    public Task<ConsultaMedica?> Handle(GetConsultaQuery request, CancellationToken cancellationToken) =>
        consultas.GetByIdAsync(request.Id, cancellationToken);
}

internal sealed class GetHistorialMascotaHandler(
    IMascotaRepository mascotas,
    IConsultaMedicaRepository consultas)
    : IRequestHandler<GetHistorialMascotaQuery, HistorialMascotaResult?>
{
    public async Task<HistorialMascotaResult?> Handle(
        GetHistorialMascotaQuery request,
        CancellationToken cancellationToken)
    {
        if (!await mascotas.ExistsAsync(request.MascotaId, cancellationToken)) return null;

        var history = await consultas.ListByMascotaAsync(request.MascotaId, cancellationToken);
        return new(history);
    }
}
