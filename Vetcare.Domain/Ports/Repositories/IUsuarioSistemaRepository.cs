using Vetcare.Domain.Entities;

namespace Vetcare.Domain.Ports.Repositories;

public interface IUsuarioSistemaRepository
{
    Task<UsuarioSistema?> GetByCorreoAsync(string correo, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
    void Add(UsuarioSistema usuario);
}
