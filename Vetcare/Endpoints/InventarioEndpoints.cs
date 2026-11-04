using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Vetcare.Data;
using Vetcare.Models;

namespace Vetcare.Endpoints;

public static class InventarioEndpoints
{
    private static readonly string[] Categorias = ["Medicamento", "Insumo"];

    public static void MapInventarioEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/inventario");

        api.MapGet("/productos", async (bool? incluirInactivos, VetCareDbContext db) =>
        {
            var consulta = db.ProductosInventario.AsNoTracking().AsQueryable();
            if (incluirInactivos != true)
                consulta = consulta.Where(p => p.Activo);

            var productos = await consulta
                .OrderBy(p => p.Nombre)
                .ToListAsync();
            return Results.Ok(productos);
        });

        api.MapGet("/productos/bajo-stock", async (VetCareDbContext db) =>
        {
            var productos = await db.ProductosInventario
                .AsNoTracking()
                .Where(p => p.Activo)
                .ToListAsync();
            return Results.Ok(productos
                .Where(p => p.StockActual <= p.StockMinimo)
                .OrderBy(p => p.Nombre));
        });

        api.MapPost("/productos", async (CrearProductoInventarioRequest input, VetCareDbContext db) =>
        {
            var errors = Validate(input);
            ValidateCategoria(input.Categoria, errors);
            var codigo = input.Codigo.Trim();
            if (await db.ProductosInventario.AnyAsync(p => p.Codigo == codigo))
                AddError(errors, nameof(input.Codigo), "Ya existe un producto con ese código.");
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            var producto = new ProductoInventario
            {
                Codigo = codigo.ToUpperInvariant(),
                Nombre = input.Nombre.Trim(),
                Categoria = input.Categoria.Trim(),
                UnidadMedida = input.UnidadMedida.Trim(),
                StockActual = input.StockInicial,
                StockMinimo = input.StockMinimo,
                ActualizadoUtc = DateTime.UtcNow
            };

            await using var transaction = await db.Database.BeginTransactionAsync();
            db.ProductosInventario.Add(producto);
            if (input.StockInicial > 0)
            {
                db.MovimientosInventario.Add(new MovimientoInventario
                {
                    ProductoInventario = producto,
                    Tipo = "Entrada",
                    Cantidad = input.StockInicial,
                    Motivo = "Stock inicial",
                    FechaMovimientoUtc = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return Results.Created($"/api/inventario/productos/{producto.Id}", producto);
        });

        api.MapPut("/productos/{id:int}", async (
            int id,
            ActualizarProductoInventarioRequest input,
            VetCareDbContext db) =>
        {
            var producto = await db.ProductosInventario.FindAsync(id);
            if (producto is null)
                return Results.NotFound();

            var errors = Validate(input);
            ValidateCategoria(input.Categoria, errors);
            var codigo = input.Codigo.Trim();
            if (await db.ProductosInventario.AnyAsync(p => p.Id != id && p.Codigo == codigo))
                AddError(errors, nameof(input.Codigo), "Ya existe un producto con ese código.");
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            producto.Codigo = codigo.ToUpperInvariant();
            producto.Nombre = input.Nombre.Trim();
            producto.Categoria = input.Categoria.Trim();
            producto.UnidadMedida = input.UnidadMedida.Trim();
            producto.StockMinimo = input.StockMinimo;
            producto.ActualizadoUtc = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(producto);
        });

        api.MapPut("/productos/{id:int}/estado", async (
            int id,
            CambiarEstadoProductoRequest input,
            VetCareDbContext db) =>
        {
            var producto = await db.ProductosInventario.FindAsync(id);
            if (producto is null)
                return Results.NotFound();
            producto.Activo = input.Activo;
            producto.ActualizadoUtc = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(producto);
        });

        api.MapDelete("/productos/{id:int}", async (int id, VetCareDbContext db) =>
        {
            var producto = await db.ProductosInventario.FindAsync(id);
            if (producto is null)
                return Results.NotFound();
            producto.Activo = false;
            producto.ActualizadoUtc = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        api.MapGet("/movimientos", async (int? productoId, VetCareDbContext db) =>
        {
            var consulta = db.MovimientosInventario
                .AsNoTracking()
                .Include(m => m.ProductoInventario)
                .AsQueryable();
            if (productoId.HasValue)
                consulta = consulta.Where(m => m.ProductoInventarioId == productoId.Value);

            var movimientos = await consulta
                .OrderByDescending(m => m.FechaMovimientoUtc)
                .Take(100)
                .Select(m => new
                {
                    m.Id,
                    m.ProductoInventarioId,
                    Producto = m.ProductoInventario!.Nombre,
                    Codigo = m.ProductoInventario.Codigo,
                    m.Tipo,
                    m.Cantidad,
                    m.Motivo,
                    m.FechaMovimientoUtc
                })
                .ToListAsync();
            return Results.Ok(movimientos);
        });

        api.MapPost("/movimientos", async (
            RegistrarMovimientoInventarioRequest input,
            VetCareDbContext db) =>
        {
            var errors = Validate(input);
            var tipo = input.Tipo.Trim();
            if (!tipo.Equals("Entrada", StringComparison.OrdinalIgnoreCase) &&
                !tipo.Equals("Salida", StringComparison.OrdinalIgnoreCase))
                AddError(errors, nameof(input.Tipo), "El tipo debe ser Entrada o Salida.");
            if (errors.Count > 0)
                return Results.ValidationProblem(errors);

            await using var transaction = await db.Database.BeginTransactionAsync();
            var producto = await db.ProductosInventario.FindAsync(input.ProductoInventarioId);
            if (producto is null)
                return Results.NotFound(new { message = "El producto no existe." });
            if (!producto.Activo)
                return Results.Conflict(new { message = "El producto está inactivo." });

            var esEntrada = tipo.Equals("Entrada", StringComparison.OrdinalIgnoreCase);
            if (!esEntrada && producto.StockActual < input.Cantidad)
                return Results.Conflict(new { message = "La salida supera el stock disponible." });

            producto.StockActual = esEntrada
                ? producto.StockActual + input.Cantidad
                : producto.StockActual - input.Cantidad;
            producto.ActualizadoUtc = DateTime.UtcNow;

            var movimiento = new MovimientoInventario
            {
                ProductoInventarioId = producto.Id,
                Tipo = esEntrada ? "Entrada" : "Salida",
                Cantidad = input.Cantidad,
                Motivo = input.Motivo.Trim(),
                FechaMovimientoUtc = DateTime.UtcNow
            };
            db.MovimientosInventario.Add(movimiento);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return Results.Created($"/api/inventario/movimientos/{movimiento.Id}", movimiento);
        });
    }

    private static void ValidateCategoria(string categoria, Dictionary<string, string[]> errors)
    {
        if (!Categorias.Contains(categoria.Trim(), StringComparer.OrdinalIgnoreCase))
            AddError(errors, nameof(CrearProductoInventarioRequest.Categoria), "Seleccione Medicamento o Insumo.");
    }

    private static Dictionary<string, string[]> Validate<T>(T model) where T : class
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results
            .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty),
                (result, member) => (member, result.ErrorMessage))
            .GroupBy(item => item.member)
            .ToDictionary(group => group.Key,
                group => group.Select(item => item.ErrorMessage ?? "El valor no es válido.").ToArray());
    }

    private static void AddError(Dictionary<string, string[]> errors, string field, string message) =>
        errors[field] = errors.TryGetValue(field, out var existing)
            ? [.. existing, message]
            : [message];
}
