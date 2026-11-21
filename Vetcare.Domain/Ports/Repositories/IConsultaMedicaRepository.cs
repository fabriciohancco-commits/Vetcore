using Vetcare.Domain.Entities;

namespace Vetcare.Domain.Ports.Repositories;

public interface IConsultaMedicaRepository
{
    Task<List<ConsultaMedica>> ListAsync(CancellationToken cancellationToken = default);
    Task<ConsultaMedica?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<ConsultaMedica>> ListByMascotaAsync(int mascotaId, CancellationToken cancellationToken = default);
    Task<bool> ExistsForMascotaAsync(int mascotaId, CancellationToken cancellationToken = default);
    Task<List<(string Veterinario, int TotalAtenciones)>> GetAtencionesPorVeterinarioAsync(
        DateTime? desde,
        DateTime? hasta,
        string? veterinario,
        CancellationToken cancellationToken = default);
    void Add(ConsultaMedica consulta);
    void Remove(ConsultaMedica consulta);
}
