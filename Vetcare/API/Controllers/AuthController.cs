using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Vetcare.Domain.Entities;
using Vetcare.Domain.Ports.Repositories;

namespace Vetcare.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IUsuarioSistemaRepository users,
    IUnitOfWork unitOfWork,
    IPasswordHasher<UsuarioSistema> passwordHasher,
    IConfiguration configuration,
    IWebHostEnvironment environment) : ControllerBase
{
    private static readonly string[] RolesPermitidos = ["Administrador", "Veterinario", "Recepcionista"];

    [AllowAnonymous]
    [HttpGet("estado")]
    public async Task<IActionResult> Estado(CancellationToken cancellationToken) =>
        Ok(new { requiereConfiguracionInicial = environment.IsDevelopment() && !await users.AnyAsync(cancellationToken) });

    [AllowAnonymous]
    [HttpPost("configuracion-inicial")]
    public async Task<IActionResult> ConfiguracionInicial(
        ConfiguracionInicialDto input, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment()) return NotFound();
        if (await users.AnyAsync(cancellationToken))
            return Conflict(new { message = "La configuración inicial ya se completó." });

        var email = input.Correo.Trim().ToLowerInvariant();
        var user = new UsuarioSistema { Correo = email, Rol = "Administrador" };
        user.PasswordHash = passwordHasher.HashPassword(user, input.Contrasena);
        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(CreateToken(user));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto input, CancellationToken cancellationToken)
    {
        var user = await users.GetByCorreoAsync(input.Correo.Trim().ToLowerInvariant(), cancellationToken);
        if (user is null || !user.Activo
            || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, input.Contrasena)
            == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Correo o contraseña incorrectos." });

        return Ok(CreateToken(user));
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost("usuarios")]
    public async Task<IActionResult> CrearUsuario(CrearUsuarioDto input, CancellationToken cancellationToken)
    {
        var email = input.Correo.Trim().ToLowerInvariant();
        if (!RolesPermitidos.Contains(input.Rol))
            return BadRequest(new { message = "El rol debe ser Administrador, Veterinario o Recepcionista." });
        if (await users.GetByCorreoAsync(email, cancellationToken) is not null)
            return Conflict(new { message = "Ya existe un usuario con ese correo." });

        var user = new UsuarioSistema { Correo = email, Rol = input.Rol };
        user.PasswordHash = passwordHasher.HashPassword(user, input.Contrasena);
        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Created("/api/auth/usuarios", new { user.Id, user.Correo, user.Rol, user.Activo, user.FechaCreacion });
    }

    private object CreateToken(UsuarioSistema user)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "VetCare";
        var audience = configuration["Jwt:Audience"] ?? "VetCare.Client";
        var signingKey = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("Configure Jwt:SigningKey mediante configuración segura.");
        if (Encoding.UTF8.GetByteCount(signingKey) < 32)
            throw new InvalidOperationException("Jwt:SigningKey debe tener al menos 32 bytes.");

        var expires = DateTime.UtcNow.AddHours(8);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Correo),
            new Claim(ClaimTypes.Name, user.Correo),
            new Claim(ClaimTypes.Role, user.Rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, DateTime.UtcNow, expires, credentials);
        return new { accessToken = new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc = expires, user.Correo, user.Rol };
    }
}

public class LoginDto
{
    [Required, EmailAddress, StringLength(160)] public string Correo { get; init; } = string.Empty;
    [Required, StringLength(128, MinimumLength = 12)] public string Contrasena { get; init; } = string.Empty;
}

public sealed class ConfiguracionInicialDto : LoginDto { }

public sealed class CrearUsuarioDto : LoginDto
{
    [Required, StringLength(30)] public string Rol { get; init; } = "Veterinario";
}
