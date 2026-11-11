using Microsoft.EntityFrameworkCore;
using Vetcare.Application.Abstractions.Persistence;
using Vetcare.Domain.Entities;

namespace Vetcare.Infrastructure.Persistence.Repositories;

public sealed class InventarioRepository(VetCareDbContext db) : IInventarioRepository
{
    public Task<List<ProductoInventario>> ListProductosAsync(bool incluirInactivos, CancellationToken ct)
    {
        var query = db.ProductosInventario.AsNoTracking().AsQueryable();
        if (!incluirInactivos) query = query.Where(p => p.Activo);
        return query.OrderBy(p => p.Nombre).ToListAsync(ct);
    }

    public Task<List<ProductoInventario>> ListarBajoStockAsync(CancellationToken ct) =>
        db.ProductosInventario.AsNoTracking().Where(p => p.Activo && p.StockActual <= p.StockMinimo)
            .OrderBy(p => p.Nombre).ToListAsync(ct);

    public Task<ProductoInventario?> FindProductoAsync(int id, CancellationToken ct) =>
        db.ProductosInventario.FindAsync([id], ct).AsTask();

    public Task<bool> ExisteCodigoAsync(string codigo, int? excluirId, CancellationToken ct) =>
        db.ProductosInventario.AnyAsync(p => p.Codigo == codigo && (!excluirId.HasValue || p.Id != excluirId.Value), ct);

    public Task<List<MovimientoInventario>> ListarMovimientosAsync(int? productoId, CancellationToken ct)
    {
        var query = db.MovimientosInventario.AsNoTracking().Include(m => m.ProductoInventario).AsQueryable();
        if (productoId.HasValue) query = query.Where(m => m.ProductoInventarioId == productoId.Value);
        return query.OrderByDescending(m => m.FechaMovimientoUtc).Take(100).ToListAsync(ct);
    }

    public void Add(ProductoInventario producto) => db.ProductosInventario.Add(producto);
    public void Add(MovimientoInventario movimiento) => db.MovimientosInventario.Add(movimiento);
}
