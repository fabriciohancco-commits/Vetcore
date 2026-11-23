using MediatR;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.Application.UseCases.Farmacia.Queries;

public sealed record ListMedicamentosQuery : IRequest<List<Medicamento>>;
public sealed record ListPrescripcionesQuery(int? MascotaId) : IRequest<List<Prescripcion>>;

internal sealed class ListMedicamentosHandler(IMedicamentoRepository repository)
    : IRequestHandler<ListMedicamentosQuery, List<Medicamento>>
{
    public Task<List<Medicamento>> Handle(ListMedicamentosQuery request, CancellationToken cancellationToken) =>
        repository.ListAsync(cancellationToken);
}

internal sealed class ListPrescripcionesHandler(IPrescripcionRepository repository)
    : IRequestHandler<ListPrescripcionesQuery, List<Prescripcion>>
{
    public Task<List<Prescripcion>> Handle(ListPrescripcionesQuery request, CancellationToken cancellationToken) =>
        repository.ListAsync(request.MascotaId, cancellationToken);
}
