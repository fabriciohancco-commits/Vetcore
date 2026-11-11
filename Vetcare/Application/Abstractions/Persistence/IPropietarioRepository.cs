using Vetcare.Domain.Entities;

namespace Vetcare.Application.Abstractions.Persistence;

public interface IPropietarioRepository
{
    Task<List<Propietario>> ListAsync(CancellationToken cancellationToken);
    Task<Propietario?> FindByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    Task<bool> HasMascotasAsync(int id, CancellationToken cancellationToken);
    void Add(Propietario propietario);
    void Remove(Propietario propietario);
}
