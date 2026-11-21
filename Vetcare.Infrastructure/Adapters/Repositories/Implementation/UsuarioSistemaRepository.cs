using Microsoft.EntityFrameworkCore;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Infrastructure.Adapters.Repositories.Implementation;

public sealed class UsuarioSistemaRepository(VetCareDbContext db) : IUsuarioSistemaRepository
{
    public Task<UsuarioSistema?> GetByCorreoAsync(string correo, CancellationToken cancellationToken = default) =>
        db.UsuariosSistema.FirstOrDefaultAsync(user => user.Correo == correo, cancellationToken);

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
        db.UsuariosSistema.AnyAsync(cancellationToken);

    public void Add(UsuarioSistema usuario) => db.UsuariosSistema.Add(usuario);
}
