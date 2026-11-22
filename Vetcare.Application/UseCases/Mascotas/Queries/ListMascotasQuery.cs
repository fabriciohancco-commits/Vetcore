using MediatR;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.Application.UseCases.Mascotas.Queries;

public sealed record ListMascotasQuery(string? Search = null) : IRequest<List<Mascota>>;

internal sealed class ListMascotasHandler(IMascotaRepository mascotas)
    : IRequestHandler<ListMascotasQuery, List<Mascota>>
{
    public Task<List<Mascota>> Handle(ListMascotasQuery request, CancellationToken cancellationToken) =>
        mascotas.ListAsync(request.Search, cancellationToken);
}
