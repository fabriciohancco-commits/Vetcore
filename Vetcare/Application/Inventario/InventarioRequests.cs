using MediatR;
using Vetcare.Application.Abstractions.Persistence;
using Vetcare.Application.Common;
using Vetcare.Domain.Entities;

namespace Vetcare.Application.Inventario;

public sealed record ListProductosInventarioQuery(bool IncluirInactivos) : IRequest<List<ProductoInventario>>;
public sealed record ListarBajoStockQuery : IRequest<List<ProductoInventario>>;
public sealed record ListarMovimientosQuery(int? ProductoId) : IRequest<List<MovimientoInventario>>;
public sealed record CrearProductoCommand(string Codigo, string Nombre, string Categoria, string UnidadMedida, decimal StockInicial, decimal StockMinimo) : IRequest<OperationResult<ProductoInventario>>;
public sealed record ActualizarProductoCommand(int Id, string Codigo, string Nombre, string Categoria, string UnidadMedida, decimal StockMinimo) : IRequest<OperationResult<ProductoInventario>>;
public sealed record CambiarEstadoProductoCommand(int Id, bool Activo) : IRequest<OperationResult<ProductoInventario>>;
public sealed record RegistrarMovimientoCommand(int ProductoId, string Tipo, decimal Cantidad, string Motivo) : IRequest<OperationResult<MovimientoInventario>>;

internal sealed class ListProductosInventarioHandler(IInventarioRepository repository) : IRequestHandler<ListProductosInventarioQuery, List<ProductoInventario>>
{
    public Task<List<ProductoInventario>> Handle(ListProductosInventarioQuery request, CancellationToken ct) => repository.ListProductosAsync(request.IncluirInactivos, ct);
}

internal sealed class ListarBajoStockHandler(IInventarioRepository repository) : IRequestHandler<ListarBajoStockQuery, List<ProductoInventario>>
{
    public Task<List<ProductoInventario>> Handle(ListarBajoStockQuery request, CancellationToken ct) => repository.ListarBajoStockAsync(ct);
}

internal sealed class ListarMovimientosHandler(IInventarioRepository repository) : IRequestHandler<ListarMovimientosQuery, List<MovimientoInventario>>
{
    public Task<List<MovimientoInventario>> Handle(ListarMovimientosQuery request, CancellationToken ct) => repository.ListarMovimientosAsync(request.ProductoId, ct);
}

internal sealed class CrearProductoHandler(IInventarioRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CrearProductoCommand, OperationResult<ProductoInventario>>
{
    private static readonly string[] Categorias = ["Medicamento", "Insumo"];
    public async Task<OperationResult<ProductoInventario>> Handle(CrearProductoCommand request, CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();
        ValidarCategoria(request.Categoria, errors);
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await repository.ExisteCodigoAsync(codigo, null, ct)) ModelValidation.AddError(errors, nameof(request.Codigo), "Ya existe un producto con ese código.");
        if (errors.Count > 0) return new(400, Errors: errors);

        var producto = new ProductoInventario { Codigo = codigo, Nombre = request.Nombre.Trim(), Categoria = request.Categoria.Trim(),
            UnidadMedida = request.UnidadMedida.Trim(), StockActual = request.StockInicial, StockMinimo = request.StockMinimo };
        await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            repository.Add(producto);
            if (request.StockInicial > 0)
                repository.Add(new MovimientoInventario { ProductoInventario = producto, Tipo = "Entrada", Cantidad = request.StockInicial, Motivo = "Stock inicial" });
            await unitOfWork.SaveChangesAsync(token);
            return true;
        }, ct);
        return new(201, producto);
    }

    internal static void ValidarCategoria(string categoria, Dictionary<string, string[]> errors)
    {
        if (!Categorias.Contains(categoria.Trim(), StringComparer.OrdinalIgnoreCase))
            ModelValidation.AddError(errors, "Categoria", "Seleccione Medicamento o Insumo.");
    }
}

internal sealed class ActualizarProductoHandler(IInventarioRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<ActualizarProductoCommand, OperationResult<ProductoInventario>>
{
    public async Task<OperationResult<ProductoInventario>> Handle(ActualizarProductoCommand request, CancellationToken ct)
    {
        var producto = await repository.FindProductoAsync(request.Id, ct);
        if (producto is null) return new(404);
        var errors = new Dictionary<string, string[]>();
        CrearProductoHandler.ValidarCategoria(request.Categoria, errors);
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        if (await repository.ExisteCodigoAsync(codigo, request.Id, ct)) ModelValidation.AddError(errors, "Codigo", "Ya existe un producto con ese código.");
        if (errors.Count > 0) return new(400, Errors: errors);
        producto.Codigo = codigo; producto.Nombre = request.Nombre.Trim(); producto.Categoria = request.Categoria.Trim();
        producto.UnidadMedida = request.UnidadMedida.Trim(); producto.StockMinimo = request.StockMinimo; producto.ActualizadoUtc = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(ct);
        return new(200, producto);
    }
}

internal sealed class CambiarEstadoProductoHandler(IInventarioRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CambiarEstadoProductoCommand, OperationResult<ProductoInventario>>
{
    public async Task<OperationResult<ProductoInventario>> Handle(CambiarEstadoProductoCommand request, CancellationToken ct)
    {
        var producto = await repository.FindProductoAsync(request.Id, ct);
        if (producto is null) return new(404);
        producto.Activo = request.Activo; producto.ActualizadoUtc = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(ct);
        return new(200, producto);
    }
}

internal sealed class RegistrarMovimientoHandler(IInventarioRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<RegistrarMovimientoCommand, OperationResult<MovimientoInventario>>
{
    public async Task<OperationResult<MovimientoInventario>> Handle(RegistrarMovimientoCommand request, CancellationToken ct)
    {
        var errors = new Dictionary<string, string[]>();
        var tipo = request.Tipo.Trim();
        if (!tipo.Equals("Entrada", StringComparison.OrdinalIgnoreCase) && !tipo.Equals("Salida", StringComparison.OrdinalIgnoreCase))
            ModelValidation.AddError(errors, "Tipo", "El tipo debe ser Entrada o Salida.");
        if (errors.Count > 0) return new(400, Errors: errors);

        return await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var producto = await repository.FindProductoAsync(request.ProductoId, token);
            if (producto is null) return new OperationResult<MovimientoInventario>(404, Message: "El producto no existe.");
            if (!producto.Activo) return new OperationResult<MovimientoInventario>(409, Message: "El producto está inactivo.");
            var entrada = tipo.Equals("Entrada", StringComparison.OrdinalIgnoreCase);
            if (!entrada && producto.StockActual < request.Cantidad)
                return new OperationResult<MovimientoInventario>(409, Message: "La salida supera el stock disponible.");
            producto.StockActual += entrada ? request.Cantidad : -request.Cantidad;
            producto.ActualizadoUtc = DateTime.UtcNow;
            var movimiento = new MovimientoInventario { ProductoInventario = producto, Tipo = entrada ? "Entrada" : "Salida",
                Cantidad = request.Cantidad, Motivo = request.Motivo.Trim(), FechaMovimientoUtc = DateTime.UtcNow };
            repository.Add(movimiento);
            await unitOfWork.SaveChangesAsync(token);
            return new OperationResult<MovimientoInventario>(201, movimiento);
        }, ct);
    }
}
