using Vetcare.Domain.Entities;

namespace Vetcare.Domain.Ports.Repositories;

public interface IMedicamentoRepository
{
    Task<List<Medicamento>> ListAsync(CancellationToken cancellationToken = default);
    Task<Medicamento?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HasPrescriptionsAsync(int id, CancellationToken cancellationToken = default);
    void Add(Medicamento medicamento);
    void Remove(Medicamento medicamento);
}
