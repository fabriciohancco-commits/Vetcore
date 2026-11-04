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
- Asociar cada mascota con su propietario.
- Validar campos, correo, teléfono, peso y fechas.
- Evitar eliminar propietarios que todavía tienen mascotas asociadas.

## Organización

```text
Vetcore/
├── Vetcare.sln
├── global.json
├── README.md
└── Vetcare/                  # Una sola aplicación y un único dominio Vetcare
    ├── API/                  # Endpoints REST
    ├── Application/          # Comandos, consultas y handlers CQRS con MediatR
    ├── Domain/Entities/      # Propietario, mascota y consulta médica
    ├── Infrastructure/      # Contexto y persistencia EF Core/SQLite
    ├── Properties/           # Perfiles de ejecución
    ├── wwwroot/              # Interfaz web
    ├── Program.cs            # Configuración e inyección de dependencias
    └── Vetcare.csproj
```

La configuración de SQLite se conserva, junto con la exclusión de archivos de compilación y preferencias locales de Rider.
