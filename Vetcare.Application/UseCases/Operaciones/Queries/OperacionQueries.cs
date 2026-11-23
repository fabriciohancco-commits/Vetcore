using MediatR;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.Application.UseCases.Operaciones.Queries;

public sealed record ListOperacionesQuery : IRequest<List<Operacion>>;

internal sealed class ListOperacionesHandler(IOperacionRepository repository)
    : IRequestHandler<ListOperacionesQuery, List<Operacion>>
{
    public Task<List<Operacion>> Handle(ListOperacionesQuery request, CancellationToken cancellationToken) =>
        repository.ListAsync(cancellationToken);
}
