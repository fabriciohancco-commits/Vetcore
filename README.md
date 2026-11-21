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
- Validar campos, correo, teléfono, peso y fechas.
- Evitar eliminar propietarios que todavía tienen mascotas asociadas.
- Evitar eliminar mascotas que tienen consultas médicas en su historial.

## Organización

VetCare sigue una organización por capas dentro de una aplicación ASP.NET Core. La estructura completa, las responsabilidades de cada carpeta y el flujo de una petición están descritos en [docs/arquitectura.md](docs/arquitectura.md).

```text
Vetcore/
├── Vetcare.sln
├── global.json
├── README.md
└── Vetcare/                  # Una sola aplicación y un único dominio Vetcare
    ├── API/                  # Controladores REST, DTOs y mapeos
    ├── Application/          # Casos de uso CQRS con MediatR
    ├── Domain/Entities/      # Propietario, mascota y consulta médica
    ├── Infrastructure/       # Contexto de datos EF Core/SQLite
    ├── Properties/           # Perfiles de ejecución
    ├── wwwroot/              # Interfaz web y recursos estáticos
    ├── Program.cs            # Composición e inicio de la aplicación
    └── Vetcare.csproj        # Proyecto web .NET 8
```

Las carpetas representan responsabilidades distintas, pero actualmente compilan en un solo proyecto .NET. La interfaz comparte `Vetcare/wwwroot/css/site.css` y mantiene la navegación entre sus cuatro pantallas. El informe de referencia muestra esas capas como proyectos separados y describe una mesa de ayuda con otras tecnologías; VetCare conserva su propio dominio veterinario, SQLite y funciones. La guía de arquitectura explica también esta diferencia y cómo leerla.

En la interfaz, `Vetcare/wwwroot/js/api.js` concentra las llamadas HTTP y utilidades compartidas. Cada pantalla usa un módulo independiente: `dashboard.js`, `consultas.js`, `historial.js` o `reportes.js`.

La base de datos SQLite se crea al iniciar la aplicación. Los archivos de compilación y las preferencias locales de Rider se excluyen de Git.
