using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vetcare.Domain.Ports.Repositories;
using Vetcare.Infrastructure.Adapters.Repositories.Implementation;
using Vetcare.Infrastructure.Persistence;

namespace Vetcare.Infrastructure;

public static class InfrastructureServicesExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<VetCareDbContext>(options =>
            options.UseSqlite(
                configuration.GetConnectionString("VetCare") ?? "Data Source=vetcare.db"));
        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<VetCareDbContext>());
        services.AddScoped<IPropietarioRepository, PropietarioRepository>();
        services.AddScoped<IMascotaRepository, MascotaRepository>();
        services.AddScoped<IConsultaMedicaRepository, ConsultaMedicaRepository>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<VetCareDbContext>();
        await db.Database.EnsureCreatedAsync();
    }
}
