using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Vetcare.Application;
using Vetcare.Domain.Entities;
using Vetcare.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega el token obtenido en POST /api/auth/login."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            }, Array.Empty<string>()
        }
    });
});
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddScoped<IPasswordHasher<UsuarioSistema>, PasswordHasher<UsuarioSistema>>();

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "VetCare";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "VetCare.Client";
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("Configure Jwt:SigningKey mediante configuración segura.");
if (Encoding.UTF8.GetByteCount(jwtSigningKey) < 32)
    throw new InvalidOperationException("Jwt:SigningKey debe tener al menos 32 bytes.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    });
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

var app = builder.Build();
QuestPDF.Settings.License = LicenseType.Community;

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

await app.Services.InitializeDatabaseAsync();
await SeedInitialAdministratorAsync(app.Services, builder.Configuration);

app.MapControllers();
app.Run();

static async Task SeedInitialAdministratorAsync(IServiceProvider services, IConfiguration configuration)
{
    var email = configuration["Auth:InitialAdminEmail"];
    var password = configuration["Auth:InitialAdminPassword"];
    if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(password)) return;
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || password.Length < 12)
        throw new InvalidOperationException("Configure Auth:InitialAdminEmail y Auth:InitialAdminPassword (mínimo 12 caracteres) juntos.");

    await using var scope = services.CreateAsyncScope();
    var users = scope.ServiceProvider.GetRequiredService<Vetcare.Domain.Ports.Repositories.IUsuarioSistemaRepository>();
    if (await users.AnyAsync(CancellationToken.None)) return;
    var user = new UsuarioSistema { Correo = email.Trim().ToLowerInvariant(), Rol = "Administrador" };
    user.PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher<UsuarioSistema>>()
        .HashPassword(user, password);
    users.Add(user);
    await scope.ServiceProvider.GetRequiredService<Vetcare.Domain.Ports.Repositories.IUnitOfWork>()
        .SaveChangesAsync(CancellationToken.None);
}

public partial class Program { }
