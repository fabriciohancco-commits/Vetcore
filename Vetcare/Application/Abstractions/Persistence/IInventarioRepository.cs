using Vetcare.Domain.Entities;

namespace Vetcare.Application.Abstractions.Persistence;

public interface IInventarioRepository
{
    Task<List<ProductoInventario>> ListProductosAsync(bool incluirInactivos, CancellationToken cancellationToken);
    Task<List<ProductoInventario>> ListarBajoStockAsync(CancellationToken cancellationToken);
    Task<ProductoInventario?> FindProductoAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExisteCodigoAsync(string codigo, int? excluirId, CancellationToken cancellationToken);
    Task<List<MovimientoInventario>> ListarMovimientosAsync(int? productoId, CancellationToken cancellationToken);
    void Add(ProductoInventario producto);
    void Add(MovimientoInventario movimiento);
}
