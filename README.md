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

Abre [http://localhost:5002](http://localhost:5002) para usar la interfaz. La base de datos SQLite se crea automáticamente al iniciar la aplicación.

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
Vetcare/
├── Vetcare.sln
├── global.json
├── README.md
└── Vetcare/
    ├── Data/                 # Contexto de Entity Framework
    ├── Models/               # Propietario y mascota
    ├── Properties/           # Perfiles de ejecución
    ├── wwwroot/              # Interfaz web
    ├── Program.cs            # Configuración y endpoints de la API
    └── Vetcare.csproj
```

La base de datos SQLite, los archivos de compilación y las preferencias locales de Rider están excluidos de Git.
