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

## Autenticación JWT

La API ahora incluye autenticación JWT completa con los siguientes endpoints:

### Endpoints de Autenticación

#### Registro de Usuario
```bash
POST /api/Auth/register
Content-Type: application/json

{
  "name": "Juan Pérez",
  "email": "juan.perez@example.com",
  "username": "jperez",
  "password": "password123"
}
```

#### Login
```bash
POST /api/Auth/login
Content-Type: application/json

{
  "username": "jperez",
  "password": "password123"
}
```

#### Obtener Usuario Actual (Autenticado)
```bash
GET /api/Auth/me
Authorization: Bearer {tu_token_jwt}
```

### Configuración JWT

Actualiza las credenciales JWT en `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "CAMBIAR-EN-PRODUCCION-MINIMO-32-CARACTERES",
    "Issuer": "BugMgrApi",
    "Audience": "BugMgrClient",
    "ExpirationMinutes": 60
  }
}
```

⚠️ **IMPORTANTE:** Cambiar `SecretKey` en producción con una clave segura de al menos 32 caracteres.

### Swagger con JWT

Swagger UI ahora incluye soporte para JWT:

1. Registra o inicia sesión para obtener un token
2. Haz clic en el botón "Authorize" en Swagger UI
3. Ingresa: `Bearer {tu_token}`
4. Ahora puedes probar endpoints protegidos

### Roles Disponibles

- **Admin**: Acceso total al sistema
- **ProductOwner**: Gestión de proyectos
- **Developer**: Desarrollo (rol por defecto al registrarse)
- **Tester**: Gestión de pruebas

### Endpoints de Prueba

Para probar la autorización:

- `GET /api/Test/public` - Acceso público
- `GET /api/Test/protected` - Requiere autenticación
- `GET /api/Test/admin-only` - Solo Admin
- `GET /api/Test/management` - Admin o ProductOwner

### Auditoría

Todas las acciones de autenticación se registran automáticamente:
- Login exitoso
- Registro de usuarios
- Incluye IP, User-Agent y detalles

Para más detalles, consulta:
- **AUTHENTICATION.md** - Documentación completa del flujo de autenticación
- **EXAMPLES.md** - Ejemplos de uso con cURL, JavaScript y Angular

## Próximos Pasos

Para completar la implementación:

1. Agregar controladores para las entidades restantes:
   - Incidents (Incidencias)
   - Sprints
   - Labels
   - Comments
   - Attachments
   - Notifications
   - AuditLogs

2. ~~Implementar autenticación JWT~~ ✅ **COMPLETADO**

3. Agregar validaciones con FluentValidation

4. Implementar patrones CQRS o Mediator para casos de uso complejos

5. Insertar roles iniciales en la base de datos (ver WebApi/Scripts/seed-roles.sql)

## Tecnologías Utilizadas

- **Swashbuckle.AspNetCore 9.0.6** - Generación de Swagger/OpenAPI
- **Microsoft.AspNetCore.OpenApi 9.0.9** - Soporte nativo OpenAPI
- **Microsoft.AspNetCore.Authentication.JwtBearer 9.0.9** - Autenticación JWT
- **Entity Framework Core 9.0.9** - ORM
- **Npgsql.EntityFrameworkCore.PostgreSQL 9.0.4** - Proveedor PostgreSQL
- **BCrypt.Net-Core 1.6.0** - Hashing seguro de contraseñas
- **Newtonsoft.Json 13.0.4** - Serialización JSON para auditoría

## Soporte

Para más información sobre Swagger, visita:
- [Documentación de Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [OpenAPI Specification](https://swagger.io/specification/)
