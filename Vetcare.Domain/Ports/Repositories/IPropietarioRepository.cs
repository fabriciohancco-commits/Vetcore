using Vetcare.Domain.Entities;

namespace Vetcare.Domain.Ports.Repositories;

public interface IPropietarioRepository
{
    Task<List<Propietario>> ListAsync(string? search, CancellationToken cancellationToken = default);
    Task<Propietario?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HasMascotasAsync(int id, CancellationToken cancellationToken = default);
    void Add(Propietario propietario);
    void Remove(Propietario propietario);
}
