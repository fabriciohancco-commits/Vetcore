using Vetcare.Domain.Entities;

namespace Vetcare.Domain.Ports.Repositories;

public interface ICitaRepository
{
    Task<List<Cita>> ListAsync(CancellationToken cancellationToken = default);
    Task<List<Cita>> ListByVeterinarioAndPeriodAsync(
        string veterinario, DateTime desde, DateTime hasta,
        CancellationToken cancellationToken = default);
    Task<Cita?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    void Add(Cita cita);
}
