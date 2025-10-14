# BugMgr API

API REST para el sistema de gestión de incidencias y proyectos Portal_Back_Nuevo.

## Características

- **ASP.NET Core 9.0** - Framework web moderno y de alto rendimiento
- **Swagger/OpenAPI** - Documentación interactiva de la API
- **Entity Framework Core** - ORM para acceso a base de datos PostgreSQL
- **Arquitectura en capas** - Separación clara entre Domain, Infrastructure, Repository y API

## Requisitos

- .NET 9.0 SDK
- PostgreSQL 12 o superior

## Configuración

1. Actualiza la cadena de conexión en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=bugmgr;Username=tu_usuario;Password=tu_contraseña"
  }
}
```

2. Asegúrate de que la base de datos PostgreSQL esté ejecutándose y accesible.

## Ejecutar la API

### Modo Development

```bash
cd API
dotnet run
```

La API estará disponible en: `http://localhost:5046`

### Modo Production

```bash
cd API
dotnet run --configuration Release
```

## Documentación Swagger

Cuando la API se ejecuta en modo **Development**, Swagger UI está disponible automáticamente en:

**URL:** `http://localhost:5046/`

![Swagger UI](https://github.com/user-attachments/assets/665a330e-5116-46ea-8ac8-472119dad604)

### Características de Swagger UI

- **Documentación interactiva** de todos los endpoints
- **Probar endpoints** directamente desde el navegador con el botón "Try it out"
- **Esquemas de datos** completos para todos los modelos
- **Respuestas de ejemplo** para cada endpoint

### Endpoints Disponibles

#### Users (Usuarios)
- `GET /api/Users` - Obtiene todos los usuarios
- `GET /api/Users/{id}` - Obtiene un usuario por ID
- `GET /api/Users/by-email/{email}` - Obtiene un usuario por email

#### Projects (Proyectos)
- `GET /api/Projects` - Obtiene todos los proyectos
- `GET /api/Projects/{id}` - Obtiene un proyecto por ID
- `GET /api/Projects/by-code/{code}` - Obtiene un proyecto por código

## Swagger JSON

El esquema OpenAPI está disponible en formato JSON en:

**URL:** `http://localhost:5046/swagger/v1/swagger.json`

Este archivo puede ser importado en herramientas como:
- Postman
- Insomnia
- Thunder Client (VS Code)
- Cualquier cliente que soporte OpenAPI 3.0

## Arquitectura

```
API/
├── Controllers/          # Controladores de la API
│   ├── UsersController.cs
│   └── ProjectsController.cs
├── Program.cs           # Configuración de la aplicación
└── appsettings.json     # Configuración de la aplicación
```

## Próximos Pasos

Esta API incluye controladores de ejemplo. Para completar la implementación:

1. Agregar controladores para las entidades restantes:
   - Incidents (Incidencias)
   - Sprints
   - Labels
   - Comments
   - Attachments
   - Notifications
   - AuditLogs

2. Implementar autenticación JWT

3. Agregar validaciones con FluentValidation

4. Implementar patrones CQRS o Mediator para casos de uso complejos

## Tecnologías Utilizadas

- **Swashbuckle.AspNetCore 9.0.6** - Generación de Swagger/OpenAPI
- **Microsoft.AspNetCore.OpenApi 9.0.9** - Soporte nativo OpenAPI
- **Entity Framework Core 9.0.9** - ORM
- **Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4** - Proveedor PostgreSQL

## Soporte

Para más información sobre Swagger, visita:
- [Documentación de Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [OpenAPI Specification](https://swagger.io/specification/)
