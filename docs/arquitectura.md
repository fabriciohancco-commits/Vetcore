# Arquitectura de VetCare

VetCare es una aplicación web ASP.NET Core organizada en capas dentro de un único proyecto .NET. Esta estructura ayuda a ubicar cada responsabilidad; no crea cuatro ensamblados independientes.

## Mapa del repositorio

```text
Vetcore/
├── Vetcare.sln
├── README.md
└── Vetcare/
    ├── API/
│   ├── Controllers/          # Endpoints HTTP
│   └── DTOs/                 # Contratos y conversión de datos
    ├── Application/
│   ├── Common/               # Resultado de operaciones y validación
│   ├── Consultas/            # Casos de uso de consultas médicas
│   ├── Mascotas/             # Casos de uso de mascotas
│   ├── Propietarios/         # Casos de uso de propietarios
│   └── Reportes/             # Consultas de reportes
    ├── Domain/Entities/      # Entidades del negocio veterinario
    ├── Infrastructure/
    │   └── Persistence/
    │       └── VetCareDbContext.cs # EF Core y relaciones SQLite
    ├── wwwroot/              # Interfaz web y recursos estáticos
    │   ├── css/site.css      # Estilos compartidos de todas las páginas
    │   ├── js/api.js         # Cliente HTTP, escape HTML y avisos
    │   ├── js/dashboard.js   # Propietarios y mascotas
    │   ├── js/consultas.js   # Listado de consultas médicas
    │   ├── js/historial.js   # Historial por mascota
    │   ├── js/reportes.js    # Filtros y resultados agregados
    │   ├── index.html        # Propietarios y mascotas
    │   ├── consultas.html    # Consultas médicas
    │   ├── historial.html   # Historial por mascota
    │   └── reportes.html     # Resumen de atenciones
    ├── Program.cs            # Configuración y composición
    └── Vetcare.csproj
```

## Responsabilidades

- **API:** recibe solicitudes HTTP, valida su forma mediante DTOs y traduce resultados a respuestas HTTP.
- **Application:** define solicitudes CQRS y sus handlers con MediatR para propietarios, mascotas, consultas e informes.
- **Domain:** contiene las entidades `Propietario`, `Mascota` y `ConsultaMedica`.
- **Infrastructure:** configura EF Core, las relaciones entre entidades y la base SQLite.
- **wwwroot:** sirve las páginas web que consumen los endpoints `/api`. Todas las vistas comparten `css/site.css`, el menú y el cliente `js/api.js`; cada pantalla mantiene su lógica en un módulo propio.

Los módulos JavaScript usan `type="module"` y comparten las utilidades de `api.js`. Esto evita repetir el manejo de errores HTTP y el escape de texto que se muestra en las tarjetas.

## Recorrido de una solicitud

```text
Navegador → API Controller → MediatR → Application Handler
          → VetCareDbContext → SQLite
          ← resultado HTTP ← respuesta del caso de uso
```

`Program.cs` registra controladores, Swagger, MediatR y el contexto SQLite. En el entorno de desarrollo habilita Swagger; al iniciar crea el esquema de datos si todavía no existe.

## Cómo se relaciona con el informe

El informe de referencia explica una mesa de ayuda y muestra API, Application, Domain e Infrastructure como proyectos separados, además de repositorios y Unit of Work. VetCare implementa un dominio distinto y mantiene esas responsabilidades en carpetas del proyecto web. Sus handlers usan actualmente `VetCareDbContext` directamente, así que la separación es organizativa y no una arquitectura hexagonal estricta. No se deben atribuir a VetCare tecnologías o funciones del informe como PostgreSQL, JWT, QuestPDF, usuarios técnicos o tickets.

Si el proyecto crece, un siguiente paso de aprendizaje sería extraer cada capa a su propio proyecto .NET e introducir contratos de persistencia para que Application no dependa de Infrastructure. Esa refactorización requiere adaptar y compilar los casos de uso; esta guía documenta el estado real antes de dar ese paso.
