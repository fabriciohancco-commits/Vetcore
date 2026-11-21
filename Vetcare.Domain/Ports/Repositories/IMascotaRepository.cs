using Vetcare.Domain.Entities;

namespace Vetcare.Domain.Ports.Repositories;

public interface IMascotaRepository
{
    Task<List<Mascota>> ListAsync(string? search, CancellationToken cancellationToken = default);
    ValueTask<Mascota?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    void Add(Mascota mascota);
    void Remove(Mascota mascota);
}
