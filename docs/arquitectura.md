# Arquitectura de VetCare

La solución separa la aplicación en cuatro proyectos .NET. Cada proyecto tiene un propósito y unas dependencias visibles en `Vetcare.sln`.

## Estructura

```text
Vetcore/
├── Vetcare.sln
├── Vetcare/                                  # Proyecto web Vetcare.API
│   ├── API/
│   │   ├── Controllers/                      # Endpoints HTTP
│   │   └── DTOs/                             # Contratos HTTP y mapeos
│   ├── Properties/                           # Perfiles de ejecución
│   ├── wwwroot/                              # Interfaz web estática
│   │   ├── css/site.css                      # Estilos compartidos
│   │   ├── js/api.js                         # Cliente HTTP y utilidades
│   │   ├── js/dashboard.js                   # Propietarios y mascotas
│   │   ├── js/consultas.js                   # Consultas médicas
│   │   ├── js/historial.js                   # Historial por mascota
│   │   ├── js/reportes.js                    # Reportes de atenciones
│   ├── Program.cs                            # Punto de composición
│   └── Vetcare.csproj
├── Vetcare.Application/
│   ├── Common/                               # Validación y resultados
│   ├── UseCases/
│   │   ├── Consultas/{Commands,Queries}/
│   │   ├── Mascotas/{Commands,Queries}/
│   │   ├── Propietarios/{Commands,Queries}/
│   │   └── Reportes/Queries/
│   └── Vetcare.Application.csproj
├── Vetcare.Domain/
│   ├── Entities/                             # Propietario, Mascota, ConsultaMedica
│   ├── Ports/Repositories/                   # Contratos por entidad y Unit of Work
│   └── Vetcare.Domain.csproj
└── Vetcare.Infrastructure/
    ├── Adapters/Repositories/Implementation/ # Repositorios EF Core
    ├── Persistence/                          # VetCareDbContext
    ├── InfrastructureServicesExtensions.cs   # Registro e inicio de SQLite
    └── Vetcare.Infrastructure.csproj
```

## Qué va en cada proyecto

- **Vetcare.API:** recibe solicitudes HTTP, valida su forma mediante DTOs, traduce respuestas y configura el inicio de la aplicación.
- **Vetcare.Application:** contiene los casos de uso CQRS. Los comandos modifican datos y las consultas recuperan datos; sus handlers dependen de contratos de Domain.
- **Vetcare.Domain:** contiene las entidades del negocio veterinario y los puertos `IPropietarioRepository`, `IMascotaRepository`, `IConsultaMedicaRepository` e `IUnitOfWork`.
- **Vetcare.Infrastructure:** implementa los puertos con EF Core y SQLite. Aquí viven el contexto, las búsquedas, la carga del historial, la agrupación de reportes y el registro de dependencias.

## Dependencias entre proyectos

```text
Vetcare.API ───────────────→ Vetcare.Application ─────→ Vetcare.Domain
     │                                                   ↑
     └──────────────────→ Vetcare.Infrastructure ───────┘
```

Application no referencia Infrastructure. Sus casos de uso hablan con interfaces; Infrastructure provee implementaciones concretas, y API conecta ambas piezas en el punto de composición.

## Recorrido de una solicitud

```text
Navegador → Controller → MediatR → Handler de Application
                                  → interfaz del repositorio
                                  → implementación EF Core → SQLite
Navegador ← respuesta HTTP ← resultado del caso de uso
```

`Program.cs` registra los handlers de Application y llama a `AddInfrastructure`. El contexto SQLite crea el esquema inicial si todavía no existe. Las páginas estáticas de `wwwroot` consumen los endpoints `/api`.

## Repositorios y Unit of Work

Los contratos no son archivos vacíos. Cada repositorio declara operaciones del dominio que necesita la aplicación:

- `IPropietarioRepository` lista y busca propietarios, consulta si existen y evita eliminarlos cuando tienen mascotas.
- `IMascotaRepository` lista y busca mascotas, las recupera por identificador y valida relaciones con propietarios.
- `IConsultaMedicaRepository` lista consultas, obtiene el historial por mascota, protege la integridad y agrupa atenciones para los reportes.
- `IUnitOfWork` confirma los cambios de un caso de uso mediante `SaveChangesAsync`.

Las clases en `Vetcare.Infrastructure/Adapters/Repositories/Implementation` ejecutan estas operaciones con `VetCareDbContext`.
