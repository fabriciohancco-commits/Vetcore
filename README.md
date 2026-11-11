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

Abre [http://localhost:5002](http://localhost:5002) para usar la interfaz. SQLite ya está configurada para la persistencia de la aplicación.

## Ejecutar desde Rider

Abre `Vetcare.sln`, selecciona el perfil **VetCare** y pulsa **Run**. El perfil utiliza el servidor integrado de ASP.NET Core y no requiere IIS Express.

## Funciones

- Registrar, listar, editar y eliminar propietarios.
- Registrar, listar, editar y eliminar mascotas.
- Asociar cada mascota con su propietario.
- Validar campos, correo, teléfono, peso y fechas.
- Evitar eliminar propietarios que todavía tienen mascotas asociadas.
- Registrar consultas, consultar historiales clínicos y generar reportes de atenciones.
- Administrar medicinas e insumos, controlar stock mínimo y registrar entradas y salidas.
- Consultar productos con stock bajo y conservar el historial de movimientos.

## Organización

```text
Vetcore/
├── Vetcare.sln
├── global.json
├── README.md
└── Vetcare/                  # Una sola aplicación y un único dominio Vetcare
    ├── API/                  # Controladores REST y DTOs de entrada/salida
    ├── Application/          # Casos de uso CQRS, validación y contratos de persistencia
    ├── Domain/Entities/      # Propietario, mascota, consulta y entidades de inventario
    ├── Infrastructure/      # Repositorios, Unit of Work y persistencia EF Core/SQLite
    ├── Properties/           # Perfiles de ejecución
    ├── wwwroot/              # Interfaz web
    ├── Program.cs            # Configuración e inyección de dependencias
    └── Vetcare.csproj
```

La estructura sigue las cuatro capas de la referencia: API, Application, Domain e Infrastructure. Los módulos de propietarios, mascotas e inventario usan interfaces de repositorio definidas en Application e implementadas en Infrastructure. Unit of Work centraliza el guardado y las transacciones de inventario. Los controladores traducen DTOs y delegan los casos de uso mediante MediatR. Consultas, historial y reportes mantienen sus handlers CQRS existentes.

La configuración de SQLite se conserva, junto con la exclusión de archivos de compilación y preferencias locales de Rider.
