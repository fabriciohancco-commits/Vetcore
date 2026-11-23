using Microsoft.EntityFrameworkCore;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Infrastructure.Adapters.Repositories.Implementation;

public sealed class MedicamentoRepository(VetCareDbContext db) : IMedicamentoRepository
{
    public Task<List<Medicamento>> ListAsync(CancellationToken cancellationToken = default) =>
        db.Medicamentos.AsNoTracking().OrderBy(m => m.Nombre).ToListAsync(cancellationToken);

    public Task<Medicamento?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Medicamentos.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default) =>
        db.Medicamentos.AnyAsync(m => m.Id == id, cancellationToken);

    public Task<bool> HasPrescriptionsAsync(int id, CancellationToken cancellationToken = default) =>
        db.Prescripciones.AnyAsync(p => p.MedicamentoId == id, cancellationToken);

    public void Add(Medicamento medicamento) => db.Medicamentos.Add(medicamento);

    public void Remove(Medicamento medicamento) => db.Medicamentos.Remove(medicamento);
}
