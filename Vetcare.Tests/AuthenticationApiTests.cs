using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vetcare.Infrastructure.Persistence;
using Xunit;

namespace Vetcare.Tests;

public sealed class AuthenticationApiTests
{
    [Fact]
    public async Task Bootstrap_login_and_role_policies_work()
    {
        await using var factory = new VetCareApiFactory();
        using var client = factory.CreateClient();

        var anonymousResponse = await client.GetAsync("/api/propietarios");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        var setupResponse = await client.PostAsJsonAsync("/api/auth/configuracion-inicial", new
        {
            correo = "admin@vetcare.test",
            contrasena = "Admin-Password-2026!"
        });
        Assert.Equal(HttpStatusCode.OK, setupResponse.StatusCode);
        var adminToken = await setupResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.False(string.IsNullOrWhiteSpace(adminToken?.AccessToken));

        var secondSetup = await client.PostAsJsonAsync("/api/auth/configuracion-inicial", new
        {
            correo = "otro@vetcare.test",
            contrasena = "Admin-Password-2026!"
        });
        Assert.Equal(HttpStatusCode.Conflict, secondSetup.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken!.AccessToken);
        var userResponse = await client.PostAsJsonAsync("/api/auth/usuarios", new
        {
            correo = "recepcion@vetcare.test",
            contrasena = "Reception-Password-2026!",
            rol = "Recepcionista"
        });
        Assert.Equal(HttpStatusCode.Created, userResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new
        {
            correo = "recepcion@vetcare.test",
            contrasena = "Reception-Password-2026!"
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var receptionist = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", receptionist!.AccessToken);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/propietarios")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/consultas")).StatusCode);
    }

    private sealed record TokenResponse(string AccessToken);

    private sealed class VetCareApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");

        public VetCareApiFactory() => _connection.Open();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = "VetCare.Tests",
                    ["Jwt:Audience"] = "VetCare.Tests.Client",
                    ["Jwt:SigningKey"] = "VetCare-Integration-Test-Signing-Key-Only-2026"
                }));
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<VetCareDbContext>>();
                services.AddDbContext<VetCareDbContext>(options => options.UseSqlite(_connection));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) _connection.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
