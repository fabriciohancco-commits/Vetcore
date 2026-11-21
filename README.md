# VetCare Manager

Proyecto final del curso de Desarrollo de Aplicaciones Empresariales Avanzado. VetCare Manager permite administrar propietarios y mascotas de una clínica veterinaria.

## Requisitos

- .NET SDK 8 o posterior
- JetBrains Rider u otro editor compatible con .NET

## Ejecutar desde la terminal

Desde la carpeta raíz del repositorio:

```powershell
dotnet run --project Vetcare/Vetcare.csproj --launch-profile VetCare
```

Abre [http://localhost:5002](http://localhost:5002) para usar la interfaz. SQLite ya está configurada para la persistencia de la aplicación.

## Ejecutar desde Rider

Abre `Vetcare.sln`, selecciona el perfil **VetCare** y pulsa **Run**. El perfil utiliza el servidor integrado de ASP.NET Core y no requiere IIS Express.

## Funciones

- Registrar, listar, editar y eliminar propietarios.
- Registrar, listar, editar y eliminar mascotas.
- Buscar propietarios por nombre, teléfono o correo y mascotas por nombre, especie o raza.
- Asociar cada mascota con su propietario.
- Agendar citas con dueño, mascota, veterinario, motivo, horario y duración; se rechazan cruces de horario para el mismo veterinario.
- Mover citas pasadas al historial sin borrarlas ni asumir que fueron atendidas.
- Validar campos, correo, teléfono, peso y fechas.
- Evitar eliminar propietarios que todavía tienen mascotas asociadas.
- Evitar eliminar mascotas que tienen consultas médicas en su historial.
- Registrar medicamentos con control de existencias, stock mínimo, vencimiento y condición de receta.
- Asociar a cada mascota las indicaciones transcritas por un veterinario; los medicamentos que requieren receta solicitan su número.
- Programar operaciones y actualizar su estado (programada, en curso, completada o cancelada).

Las indicaciones clínicas son registradas por el profesional veterinario. El sistema no recomienda fármacos ni calcula dosis.

## Organización

VetCare está separado en cuatro proyectos .NET: API, Application, Domain e Infrastructure. La estructura completa, las dependencias entre proyectos y el recorrido de una petición están descritos en [docs/arquitectura.md](docs/arquitectura.md).

```text
Vetcore/
├── Vetcare.sln
├── global.json
├── README.md
├── Vetcare/                  # Vetcare.API: HTTP, composición e interfaz web
├── Vetcare.Application/      # Casos de uso y handlers CQRS
├── Vetcare.Domain/           # Entidades y contratos de repositorio
└── Vetcare.Infrastructure/   # EF Core, SQLite y adaptadores
```

Cada proyecto tiene su propio `.csproj` y referencias explícitas. Los contratos de los repositorios están en Domain; Application los usa en sus casos de uso; Infrastructure implementa esos contratos con EF Core; API conecta los servicios y expone los endpoints. La interfaz comparte `Vetcare/wwwroot/css/site.css` y mantiene navegación entre pacientes, citas, consultas, historial, farmacia, operaciones y reportes.

En la interfaz, `Vetcare/wwwroot/js/api.js` concentra las llamadas HTTP y utilidades compartidas. Cada pantalla usa un módulo independiente: `dashboard.js`, `citas.js`, `consultas.js`, `historial.js`, `farmacia.js`, `operaciones.js` o `reportes.js`.

La base de datos SQLite se crea al iniciar la aplicación y conserva localmente propietarios, mascotas, medicamentos y recetas. El archivo de base de datos y sus archivos auxiliares se excluyen de Git; no se incluyen datos de clientes ni pacientes en los commits.
# VetCare Manager

Sistema de gestión veterinaria con API REST en ASP.NET Core .NET 8, arquitectura por capas, casos de uso CQRS con MediatR, EF Core y SQLite. Incluye módulos de propietarios, mascotas, consultas clínicas, citas, farmacia, operaciones y reportes.

La documentación para ejecutar la API, autenticación JWT y roles, rutas, ejemplos de solicitudes, reglas de negocio y migraciones está en [docs/api.md](docs/api.md). El diseño de capas se describe en [docs/arquitectura.md](docs/arquitectura.md). Los ejemplos para probar endpoints desde Rider o Visual Studio están en `Vetcare/Vetcare.http`.
