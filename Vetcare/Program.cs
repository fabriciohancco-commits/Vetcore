using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Vetcare.Data;
using Vetcare.Endpoints;
using Vetcare.Models;
using Vetcare.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<VetCareDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("VetCare") ?? "Data Source=vetcare.db"));
builder.Services.AddScoped<ReporteService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VetCareDbContext>();
    VetCareDatabaseInitializer.Initialize(db);
}

var api = app.MapGroup("/api");

app.MapInventarioEndpoints();

api.MapGet("/propietarios", async (VetCareDbContext db) =>
    Results.Ok(await db.Propietarios.AsNoTracking().OrderBy(p => p.Apellidos).ThenBy(p => p.Nombres).ToListAsync()));

api.MapPost("/propietarios", async (Propietario propietario, VetCareDbContext db) =>
{
    var errors = Validate(propietario);
    if (errors.Count > 0) return Results.ValidationProblem(errors);
    db.Propietarios.Add(propietario);
    await db.SaveChangesAsync();
    return Results.Created($"/api/propietarios/{propietario.Id}", propietario);
});

api.MapPut("/propietarios/{id:int}", async (int id, Propietario input, VetCareDbContext db) =>
{
    var actual = await db.Propietarios.FindAsync(id);
    if (actual is null) return Results.NotFound();
    input.Id = id;
    var errors = Validate(input);
    if (errors.Count > 0) return Results.ValidationProblem(errors);
    actual.Nombres = input.Nombres;
    actual.Apellidos = input.Apellidos;
    actual.Telefono = input.Telefono;
    actual.Correo = input.Correo;
    actual.Direccion = input.Direccion;
    await db.SaveChangesAsync();
    return Results.Ok(actual);
});

api.MapDelete("/propietarios/{id:int}", async (int id, VetCareDbContext db) =>
{
    var propietario = await db.Propietarios.FindAsync(id);
    if (propietario is null) return Results.NotFound();
    if (await db.Mascotas.AnyAsync(m => m.PropietarioId == id))
        return Results.Conflict(new { message = "El propietario tiene mascotas registradas. Reasígnelas o elimínelas antes de continuar." });
    db.Propietarios.Remove(propietario);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

api.MapGet("/mascotas", async (VetCareDbContext db) =>
    Results.Ok(await db.Mascotas.AsNoTracking().OrderBy(m => m.Nombre).ToListAsync()));

api.MapPost("/mascotas", async (Mascota mascota, VetCareDbContext db) =>
{
    var errors = Validate(mascota);
    if (mascota.FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
        AddError(errors, nameof(mascota.FechaNacimiento), "La fecha de nacimiento no puede ser futura.");
    if (!await db.Propietarios.AnyAsync(p => p.Id == mascota.PropietarioId))
        AddError(errors, nameof(mascota.PropietarioId), "El propietario seleccionado no existe.");
    if (errors.Count > 0) return Results.ValidationProblem(errors);
    db.Mascotas.Add(mascota);
    await db.SaveChangesAsync();
    return Results.Created($"/api/mascotas/{mascota.Id}", mascota);
});

api.MapPut("/mascotas/{id:int}", async (int id, Mascota input, VetCareDbContext db) =>
{
    var actual = await db.Mascotas.FindAsync(id);
    if (actual is null) return Results.NotFound();
    input.Id = id;
    var errors = Validate(input);
    if (input.FechaNacimiento > DateOnly.FromDateTime(DateTime.Today))
        AddError(errors, nameof(input.FechaNacimiento), "La fecha de nacimiento no puede ser futura.");
    if (!await db.Propietarios.AnyAsync(p => p.Id == input.PropietarioId))
        AddError(errors, nameof(input.PropietarioId), "El propietario seleccionado no existe.");
    if (errors.Count > 0) return Results.ValidationProblem(errors);
    actual.Nombre = input.Nombre;
    actual.Especie = input.Especie;
    actual.Raza = input.Raza;
    actual.FechaNacimiento = input.FechaNacimiento;
    actual.PesoKg = input.PesoKg;
    actual.Observaciones = input.Observaciones;
    actual.PropietarioId = input.PropietarioId;
    await db.SaveChangesAsync();
    return Results.Ok(actual);
});

api.MapDelete("/mascotas/{id:int}", async (int id, VetCareDbContext db) =>
{
    var mascota = await db.Mascotas.FindAsync(id);
    if (mascota is null) return Results.NotFound();
    db.Mascotas.Remove(mascota);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// MODULO DE HISTORIAL CLINICO - JUAN MACHACA

// Registrar una consulta médica
api.MapPost("/consultas", async (
    ConsultaMedica consulta,
    VetCareDbContext db) =>
{
    var errors = Validate(consulta);

    if (consulta.FechaConsulta > DateTime.Now)
    {
        AddError(
            errors,
            nameof(consulta.FechaConsulta),
            "La fecha de consulta no puede ser futura.");
    }

    if (!await db.Mascotas.AnyAsync(
        m => m.Id == consulta.MascotaId))
    {
        AddError(
            errors,
            nameof(consulta.MascotaId),
            "La mascota seleccionada no existe.");
    }

    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    db.ConsultasMedicas.Add(consulta);

    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/consultas/{consulta.Id}",
        consulta);
});

// Consultar todas las fichas médicas
api.MapGet("/consultas", async (VetCareDbContext db) =>
{
    var consultas = await db.ConsultasMedicas
        .AsNoTracking()
        .Include(c => c.Mascota)
        .OrderByDescending(c => c.FechaConsulta)
        .ToListAsync();

    return Results.Ok(consultas);
});

// Consultar una ficha médica por ID
api.MapGet("/consultas/{id:int}", async (
    int id,
    VetCareDbContext db) =>
{
    var consulta = await db.ConsultasMedicas
        .AsNoTracking()
        .Include(c => c.Mascota)
        .FirstOrDefaultAsync(c => c.Id == id);

    return consulta is null
        ? Results.NotFound()
        : Results.Ok(consulta);
});

// Consultar historial clínico de una mascota
api.MapGet("/mascotas/{mascotaId:int}/historial", async (
    int mascotaId,
    VetCareDbContext db) =>
{
    var existe = await db.Mascotas
        .AnyAsync(m => m.Id == mascotaId);

    if (!existe)
        return Results.NotFound(
            new { message = "Mascota no encontrada." });

    var historial = await db.ConsultasMedicas
        .AsNoTracking()
        .Where(c => c.MascotaId == mascotaId)
        .OrderByDescending(c => c.FechaConsulta)
        .ToListAsync();

    return Results.Ok(historial);
});

api.MapPut("/consultas/{id:int}", async (
    int id,
    ConsultaMedica input,
    VetCareDbContext db) =>
{
    var actual = await db.ConsultasMedicas.FindAsync(id);

    if (actual is null)
        return Results.NotFound();

    var errors = Validate(input);

    if (input.FechaConsulta > DateTime.Now)
    {
        AddError(
            errors,
            nameof(input.FechaConsulta),
            "La fecha de consulta no puede ser futura.");
    }

    if (!await db.Mascotas.AnyAsync(
            m => m.Id == input.MascotaId))
    {
        AddError(
            errors,
            nameof(input.MascotaId),
            "La mascota seleccionada no existe.");
    }

    if (errors.Count > 0)
        return Results.ValidationProblem(errors);

    actual.MascotaId = input.MascotaId;
    actual.FechaConsulta = input.FechaConsulta;
    actual.VeterinarioNombre = input.VeterinarioNombre;
    actual.Motivo = input.Motivo;
    actual.Diagnostico = input.Diagnostico;
    actual.Tratamiento = input.Tratamiento;
    actual.VacunasAplicadas = input.VacunasAplicadas;

    await db.SaveChangesAsync();

    return Results.Ok(actual);
});

// Eliminar una ficha médica
api.MapDelete("/consultas/{id:int}", async (
    int id,
    VetCareDbContext db) =>
{
    var consulta = await db.ConsultasMedicas.FindAsync(id);

    if (consulta is null)
        return Results.NotFound();

    db.ConsultasMedicas.Remove(consulta);

    await db.SaveChangesAsync();

    return Results.NoContent();
});

// MODÚLO DE REPORTES - JUAN MACHACA

api.MapGet("/reportes/atenciones", async (
    DateTime? desde,
    DateTime? hasta,
    string? veterinario,
    ReporteService reporteService) =>
{
    if (desde.HasValue &&
        hasta.HasValue &&
        desde.Value.Date > hasta.Value.Date)
    {
        return Results.BadRequest(new
        {
            message = "La fecha inicial no puede ser posterior a la fecha final."
        });
    }

    var reporte = await reporteService.ObtenerReporteAsync(
        desde,
        hasta,
        veterinario);

    return Results.Ok(reporte);
});

app.Run();

static Dictionary<string, string[]> Validate<T>(T model) where T : class
{
    var context = new ValidationContext(model);
    var results = new List<ValidationResult>();
    Validator.TryValidateObject(model, context, results, validateAllProperties: true);
    return results
        .SelectMany(result => result.MemberNames.DefaultIfEmpty(string.Empty), (result, member) => (member, result.ErrorMessage))
        .GroupBy(item => item.member)
        .ToDictionary(group => group.Key, group => group.Select(item => item.ErrorMessage ?? "El valor no es válido.").ToArray());
}

static void AddError(Dictionary<string, string[]> errors, string field, string message) =>
    errors[field] = errors.TryGetValue(field, out var existing) ? [.. existing, message] : [message];
