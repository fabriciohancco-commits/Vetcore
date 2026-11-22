using MediatR;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.Application.UseCases.Propietarios.Queries;

public sealed record ListPropietariosQuery(string? Search = null) : IRequest<List<Propietario>>;

internal sealed class ListPropietariosHandler(IPropietarioRepository propietarios)
    : IRequestHandler<ListPropietariosQuery, List<Propietario>>
{
    public Task<List<Propietario>> Handle(ListPropietariosQuery request, CancellationToken cancellationToken) =>
        propietarios.ListAsync(request.Search, cancellationToken);
}
