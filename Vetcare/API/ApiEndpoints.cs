using MediatR;
using Vetcare.Application.Common;
using Vetcare.Application.Consultas;
using Vetcare.Application.Mascotas;
using Vetcare.Application.Propietarios;
using Vetcare.Application.Reportes;
using Vetcare.Domain.Entities;

namespace Vetcare.API;

public static class ApiEndpoints
{
    public static void Map(WebApplication app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/propietarios", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new ListPropietariosQuery(), ct)));

        api.MapPost("/propietarios", async (Propietario input, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreatePropietarioCommand(input), ct);
            return result.StatusCode == 201
                ? Results.Created($"/api/propietarios/{result.Value!.Id}", result.Value)
                : ToHttpResult(result);
        });

        api.MapPut("/propietarios/{id:int}", async (int id, Propietario input, ISender sender, CancellationToken ct) =>
            ToHttpResult(await sender.Send(new UpdatePropietarioCommand(id, input), ct)));

        api.MapDelete("/propietarios/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            ToHttpResult(await sender.Send(new DeletePropietarioCommand(id), ct)));

        api.MapGet("/mascotas", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new ListMascotasQuery(), ct)));

        api.MapPost("/mascotas", async (Mascota input, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateMascotaCommand(input), ct);
            return result.StatusCode == 201
                ? Results.Created($"/api/mascotas/{result.Value!.Id}", result.Value)
                : ToHttpResult(result);
        });

        api.MapPut("/mascotas/{id:int}", async (int id, Mascota input, ISender sender, CancellationToken ct) =>
            ToHttpResult(await sender.Send(new UpdateMascotaCommand(id, input), ct)));

        api.MapDelete("/mascotas/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            ToHttpResult(await sender.Send(new DeleteMascotaCommand(id), ct)));

        api.MapPost("/consultas", async (ConsultaMedica input, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateConsultaCommand(input), ct);
            return result.StatusCode == 201
                ? Results.Created($"/api/consultas/{result.Value!.Id}", result.Value)
                : ToHttpResult(result);
        });

        api.MapGet("/consultas", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new ListConsultasQuery(), ct)));

        api.MapGet("/consultas/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetConsultaQuery(id), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        api.MapGet("/mascotas/{mascotaId:int}/historial", async (int mascotaId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetHistorialMascotaQuery(mascotaId), ct);
            return result is null
                ? Results.NotFound(new { message = "Mascota no encontrada." })
                : Results.Ok(result.Consultas);
        });

        api.MapPut("/consultas/{id:int}", async (int id, ConsultaMedica input, ISender sender, CancellationToken ct) =>
            ToHttpResult(await sender.Send(new UpdateConsultaCommand(id, input), ct)));

        api.MapDelete("/consultas/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
            ToHttpResult(await sender.Send(new DeleteConsultaCommand(id), ct)));

        api.MapGet("/reportes/atenciones", async (
            DateTime? desde, DateTime? hasta, string? veterinario, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetReporteAtencionesQuery(desde, hasta, veterinario), ct);
            return result.StatusCode == 200 ? Results.Ok(result.Value) : ToHttpResult(result);
        });
    }

    private static IResult ToHttpResult<T>(OperationResult<T> result) => result.StatusCode switch
    {
        200 => Results.Ok(result.Value),
        204 => Results.NoContent(),
        400 when result.Errors is not null => Results.ValidationProblem(result.Errors),
        400 => Results.BadRequest(new { message = result.Message }),
        404 => Results.NotFound(),
        409 => Results.Conflict(new { message = result.Message }),
        _ => Results.StatusCode(result.StatusCode)
    };
}
