# VetCare Manager

Proyecto final del curso de Desarrollo de Aplicaciones Empresariales Avanzado. VetCare Manager permite administrar propietarios, mascotas, consultas clínicas e inventario de una clínica veterinaria.

## Requisitos

- .NET SDK 8 o posterior
- JetBrains Rider u otro editor compatible con .NET

## Ejecutar desde la terminal

Desde la carpeta raíz del repositorio:

```powershell
dotnet run --project Vetcare/Vetcare.csproj --launch-profile VetCare
```

Abre [http://localhost:5002](http://localhost:5002) para usar la interfaz. La base de datos SQLite se crea y actualiza al iniciar la aplicación.

## Ejecutar desde Rider

Abre `Vetcare.sln`, selecciona el perfil **VetCare** y pulsa **Run**. El perfil utiliza el servidor integrado de ASP.NET Core y no requiere IIS Express.

## Funciones

- Registrar, listar, editar y eliminar propietarios y mascotas.
- Registrar consultas médicas y consultar historial por mascota.
- Generar reportes por fechas y veterinario.
- Administrar medicinas e insumos, establecer stock mínimo y registrar entradas y salidas.
- Conservar el historial de movimientos y bloquear salidas que superen las existencias disponibles.

## Organización

```text
Vetcare/
├── Vetcare.sln
├── global.json
├── README.md
└── Vetcare/
    ├── Data/                 # Contexto e inicialización de la base de datos
    ├── Endpoints/             # Rutas del módulo de inventario
    ├── Models/                # Entidades y solicitudes
    ├── Services/              # Lógica de reportes
    ├── Properties/            # Perfiles de ejecución
    ├── wwwroot/               # Interfaz web
    ├── Program.cs             # Configuración de la aplicación y API
    └── Vetcare.csproj
```

La base de datos SQLite, los archivos de compilación y las preferencias locales de Rider están excluidos de Git.