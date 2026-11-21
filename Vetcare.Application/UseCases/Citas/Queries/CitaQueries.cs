using MediatR;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.Application.UseCases.Citas.Queries;

public sealed record ListCitasQuery : IRequest<List<Cita>>;

internal sealed class ListCitasHandler(ICitaRepository repository)
    : IRequestHandler<ListCitasQuery, List<Cita>>
{
    public Task<List<Cita>> Handle(ListCitasQuery request, CancellationToken cancellationToken) =>
        repository.ListAsync(cancellationToken);
}
