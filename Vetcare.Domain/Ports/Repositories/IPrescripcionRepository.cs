using Vetcare.Domain.Entities;

namespace Vetcare.Domain.Ports.Repositories;

public interface IPrescripcionRepository
{
    Task<List<Prescripcion>> ListAsync(int? mascotaId = null, CancellationToken cancellationToken = default);
    Task<Prescripcion?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    void Add(Prescripcion prescripcion);
}
