# API de VetCare

La API está construida en ASP.NET Core .NET 8. Los controladores reciben y validan HTTP, MediatR deriva los comandos y consultas hacia los casos de uso y los repositorios implementan persistencia mediante EF Core y SQLite. Los endpoints devuelven JSON salvo el reporte PDF.

## Acceso

La aplicación no tiene inicio de sesión. Todas las rutas de la API quedan públicas y permiten las operaciones que expone cada endpoint. En un servicio público, cualquier persona con el enlace podría leer, crear, cambiar o eliminar registros; usa solo datos ficticios y de demostración.

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| GET, POST, PUT `{id}`, DELETE `{id}` | `/api/propietarios` | Buscar/listar, crear, actualizar y eliminar |
| GET, POST, PUT `{id}`, DELETE `{id}` | `/api/mascotas` | Buscar/listar, crear, actualizar y eliminar |
| GET | `/api/mascotas/{id}/historial` | Consultas asociadas a una mascota |
| GET, POST, PUT `{id}`, DELETE `{id}` | `/api/consultas` | Gestionar consultas clínicas |
| GET, POST | `/api/citas` | Listar y reservar; valida conflictos de horario |
| PUT | `/api/citas/{id}/estado` | Cambiar estado de cita |
| GET, POST, PUT | `/api/farmacia/medicamentos` | Consultar y mantener el catálogo |
| GET, POST | `/api/farmacia/prescripciones` | Consultar historial y registrar dispensaciones |
| GET, POST, PUT estado, DELETE | `/api/operaciones` | Programar y gestionar procedimientos |
| GET | `/api/reportes/atenciones` | Resumen JSON por fechas y veterinario |
| GET | `/api/reportes/atenciones/pdf` | El mismo reporte como descarga PDF |

Los IDs de los ejemplos deben corresponder a registros existentes. Las solicitudes inválidas reciben `400`, recursos inexistentes `404` y conflictos de agenda o inventario `409`.

## Migraciones e inventario

La base evoluciona mediante migraciones EF Core. Al iniciar, el sistema prepara el esquema de una base heredada de VetCare y registra la migración base antes de aplicar cambios posteriores. No borres `vetcare.db` para instalar una versión nueva; conserva una copia antes de actualizar una instancia con datos importantes.

Al dispensar una receta, `UnidadesEntregadas` se valida contra existencias, se descuenta del medicamento y se guarda dentro de la misma operación de EF. El stock tiene control de concurrencia; si otra solicitud actualiza el medicamento primero, la API devuelve `409`.

## Ejecutar y explorar

Abre `Vetcare.sln` y ejecuta el proyecto `Vetcare.API`. En Development, Swagger queda disponible en `/swagger`. También puedes abrir `Vetcare/Vetcare.http` en JetBrains Rider o Visual Studio; los ejemplos no requieren token.

La colección HTTP cubre propietarios, consultas, citas, farmacia, descuento de stock y reportes. Primero registra datos base (propietario, mascota y medicamento), luego usa los IDs devueltos en las operaciones dependientes.

El proyecto usa QuestPDF con su licencia Community configurada en el arranque; revisa la elegibilidad/licencia aplicable si VetCare se distribuye o utiliza fuera de un contexto de aprendizaje.
