# API de VetCare

La API está construida en ASP.NET Core .NET 8. Los controladores reciben y validan HTTP, MediatR deriva los comandos y consultas hacia los casos de uso y los repositorios implementan persistencia mediante EF Core y SQLite. Los endpoints devuelven JSON salvo el reporte PDF.

## Autenticación y roles

Todas las rutas requieren un token JWT, excepto `GET /api/auth/estado`, `POST /api/auth/configuracion-inicial` y `POST /api/auth/login`. Enviar el token como `Authorization: Bearer <token>`. Las contraseñas se guardan con `PasswordHasher`, y los tokens validan firma, emisor, audiencia y expiración.

| Rol | Acceso principal |
|---|---|
| Administrador | Todos los módulos, gestión de cuentas y borrado de operaciones |
| Veterinario | Pacientes, consultas, historial, recetas, operaciones y reportes |
| Recepcionista | Propietarios, mascotas, citas y catálogo de medicamentos |

En Development, la pantalla de inicio permite crear el primer administrador solo cuando no existen usuarios. Para iniciar una instancia desplegada, establece `Auth__InitialAdminEmail` y `Auth__InitialAdminPassword` como variables de entorno antes del primer arranque; exige una contraseña de al menos 12 caracteres. `Jwt__SigningKey` debe ser una clave secreta de al menos 32 bytes; configura también `Jwt__Issuer`, `Jwt__Audience` y `ConnectionStrings__VetCare`. No guardes secretos de producción en `appsettings.json`.

## Endpoints

| Método | Ruta | Descripción / acceso |
|---|---|---|
| GET, POST | `/api/auth/estado`, `/api/auth/login` | Estado de bootstrap e inicio de sesión, anónimos |
| POST | `/api/auth/configuracion-inicial` | Crear primer administrador, solo Development y una vez |
| POST | `/api/auth/usuarios` | Crear usuario, Administrador |
| GET, POST, PUT `{id}`, DELETE `{id}` | `/api/propietarios` | Buscar/listar, crear, actualizar y eliminar |
| GET, POST, PUT `{id}`, DELETE `{id}` | `/api/mascotas` | Buscar/listar, crear, actualizar y eliminar |
| GET | `/api/mascotas/{id}/historial` | Consultas asociadas a una mascota |
| GET, POST, PUT `{id}`, DELETE `{id}` | `/api/consultas` | Gestionar consultas clínicas, Veterinario/Administrador |
| GET, POST | `/api/citas` | Listar y reservar; valida conflictos de horario |
| PUT | `/api/citas/{id}/estado` | Cambiar estado de cita |
| GET, POST, PUT | `/api/farmacia/medicamentos` | Consultar catálogo; altas/cambios por Administrador/Recepcionista |
| GET, POST | `/api/farmacia/prescripciones` | Historial y dispensación por Veterinario/Administrador |
| GET, POST, PUT estado, DELETE | `/api/operaciones` | Programar y gestionar procedimientos con restricciones por rol |
| GET | `/api/reportes/atenciones` | Resumen JSON por fechas y veterinario |
| GET | `/api/reportes/atenciones/pdf` | El mismo reporte como descarga PDF |

Los IDs de los ejemplos deben corresponder a registros existentes. Las solicitudes inválidas reciben `400`, recursos inexistentes `404`, conflictos de agenda o inventario `409` y peticiones sin autenticar `401`. Los roles insuficientes reciben `403`.

## Migraciones e inventario

La base evoluciona mediante migraciones EF Core. Al iniciar, el sistema prepara el esquema de una base heredada de VetCare y registra la migración base antes de aplicar cambios posteriores. No borres `vetcare.db` para instalar una versión nueva; conserva una copia antes de actualizar una instancia con datos importantes.

Al dispensar una receta, `UnidadesEntregadas` se valida contra existencias, se descuenta del medicamento y se guarda dentro de la misma operación de EF. El stock tiene control de concurrencia; si otra solicitud actualiza el medicamento primero, la API devuelve `409`.

## Ejecutar y explorar

Abre `Vetcare.sln` y ejecuta el proyecto `Vetcare.API`. En Development, Swagger queda disponible en `/swagger`. En Swagger usa **Authorize** para pegar el JWT de `/api/auth/login`. También puedes abrir `Vetcare/Vetcare.http` en JetBrains Rider o Visual Studio; sigue el ejemplo de login y copia el `accessToken` en la variable `@token`.

La colección HTTP cubre autenticación, propietarios, consultas, citas, farmacia, descuento de stock y reportes. Primero registra datos base (propietario, mascota y medicamento), luego usa los IDs devueltos en las operaciones dependientes.

El proyecto `Vetcare.Tests` incluye una prueba de integración de autenticación, bloqueo anónimo, bootstrap de una sola vez y permisos por rol. Ejecútala desde la solución con `dotnet test Vetcare.sln`.

El proyecto usa QuestPDF con su licencia Community configurada en el arranque; revisa la elegibilidad/licencia aplicable si VetCare se distribuye o utiliza fuera de un contexto de aprendizaje.
