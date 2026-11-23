using Vetcare.Domain.Entities;

namespace Vetcare.Domain.Ports.Repositories;

public interface IOperacionRepository
{
    Task<List<Operacion>> ListAsync(CancellationToken cancellationToken = default);
    Task<Operacion?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    void Add(Operacion operacion);
    void Remove(Operacion operacion);
}
