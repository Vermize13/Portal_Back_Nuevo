# WebApi - Portal Backend

API REST para el sistema de gestión de incidencias (Bug Manager).

## Requisitos Previos

- .NET 9.0 SDK
- PostgreSQL 12 o superior
- Una base de datos PostgreSQL creada (nombre sugerido: `bugmgr`)

## Configuración Inicial

### 1. Configurar la Cadena de Conexión

Editar `appsettings.json` y actualizar la cadena de conexión a PostgreSQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=bugmgr;Username=postgres;Password=tu_password"
  }
}
```

### 2. Aplicar Migraciones de Base de Datos

Desde el directorio raíz del proyecto (donde está BugMgr.sln):

```bash
# Navegar al directorio Infrastructure
cd Infrastructure

# Aplicar migraciones
dotnet ef database update

# O desde el directorio raíz
dotnet ef database update --project Infrastructure
```

### 3. Insertar Roles Iniciales

Ejecutar el script SQL para insertar los roles del sistema:

```bash
psql -h localhost -U postgres -d bugmgr -f WebApi/Scripts/seed-roles.sql
```

O conectarse a la base de datos y ejecutar manualmente:

```sql
INSERT INTO "Roles" ("Id", "Code", "Name", "Description")
VALUES 
    ('a0000000-0000-0000-0000-000000000001', 'Admin', 'Administrador', 'Administrador del sistema con acceso total'),
    ('a0000000-0000-0000-0000-000000000002', 'ProductOwner', 'Product Owner', 'Product Owner con permisos de gestión de proyectos'),
    ('a0000000-0000-0000-0000-000000000003', 'Developer', 'Desarrollador', 'Desarrollador con permisos básicos de desarrollo'),
    ('a0000000-0000-0000-0000-000000000004', 'Tester', 'Tester', 'Tester con permisos de gestión de incidencias y pruebas')
ON CONFLICT ("Id") DO NOTHING;
```

### 4. Configurar JWT Secret Key

**IMPORTANTE:** Cambiar la clave secreta JWT en producción.

En `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "CAMBIAR-ESTO-EN-PRODUCCION-USAR-CLAVE-SEGURA-DE-AL-MENOS-32-CARACTERES",
    "Issuer": "BugMgrApi",
    "Audience": "BugMgrClient",
    "ExpirationMinutes": 60
  }
}
```

Para generar una clave segura en producción, usar:

```bash
openssl rand -base64 32
```

## Ejecutar la API

Desde el directorio WebApi:

```bash
dotnet run
```

La API estará disponible en:
- HTTP: `http://localhost:5000`

## Endpoints de Autenticación

### Registro de Usuario

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Juan Pérez",
    "email": "juan.perez@example.com",
    "username": "jperez",
    "password": "password123"
  }'
```

### Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "jperez",
    "password": "password123"
  }'
```

### Obtener Usuario Actual (Requiere Autenticación)

```bash
curl -X GET http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Estructura del Proyecto

```
WebApi/
├── Controllers/         # Controladores de la API
│   └── AuthController.cs
├── DTOs/               # Data Transfer Objects
│   ├── LoginRequest.cs
│   ├── RegisterRequest.cs
│   ├── AuthResponse.cs
│   └── JwtSettings.cs
├── Services/           # Lógica de negocio
│   ├── AuthService.cs
│   ├── IAuthService.cs
│   ├── AuditService.cs
│   └── IAuditService.cs
├── Middleware/         # Middleware personalizado
│   └── RoleAuthorizationAttribute.cs
├── Scripts/            # Scripts SQL
│   └── seed-roles.sql
├── Program.cs          # Configuración de la aplicación
├── appsettings.json    # Configuración
└── README.md
```

## Seguridad

### Contraseñas

- Las contraseñas se hashean usando **BCrypt** antes de almacenarse
- BCrypt genera un salt único para cada contraseña
- Las contraseñas nunca se almacenan en texto plano

### JWT

- Los tokens expiran después de 60 minutos (configurable)
- Los tokens incluyen claims de usuario y roles
- La firma del token se valida en cada request

### Auditoría

- Todas las acciones de autenticación se registran en la tabla `AuditLogs`
- Se almacena: acción, usuario, IP, User-Agent, timestamp

## Roles del Sistema

1. **Admin**: Acceso total al sistema
2. **ProductOwner**: Gestión de proyectos y sprints
3. **Developer**: Desarrollo y gestión de incidencias
4. **Tester**: Gestión de pruebas e incidencias

## Protección de Endpoints

### Usando `[Authorize]`

```csharp
[HttpGet("protected")]
[Authorize]
public IActionResult ProtectedEndpoint()
{
    return Ok("Requiere autenticación");
}
```

### Usando `[RoleAuthorization]`

```csharp
[HttpGet("admin-only")]
[RoleAuthorization("Admin")]
public IActionResult AdminOnly()
{
    return Ok("Solo administradores");
}

[HttpGet("admin-or-po")]
[RoleAuthorization("Admin", "ProductOwner")]
public IActionResult AdminOrPO()
{
    return Ok("Admin o Product Owner");
}
```

## Desarrollo

### Build

```bash
dotnet build
```

### Ejecutar en modo desarrollo

```bash
dotnet run --environment Development
```

### Ver logs

Los logs se muestran en la consola durante la ejecución.

## Documentación Adicional

Ver [AUTHENTICATION.md](../AUTHENTICATION.md) para documentación detallada del flujo de autenticación.

## Troubleshooting

### Error: "Cannot connect to database"

Verificar:
1. PostgreSQL está ejecutándose
2. La base de datos existe
3. Las credenciales en `appsettings.json` son correctas

### Error: "Role 'Developer' not found"

Ejecutar el script `seed-roles.sql` para insertar los roles iniciales.

### Error: "Invalid token"

El token puede haber expirado o ser inválido. Obtener un nuevo token mediante login.

## Próximos Pasos

- Implementar endpoints CRUD para proyectos
- Implementar endpoints CRUD para incidencias
- Agregar validación de complejidad de contraseñas
- Implementar rate limiting para prevenir ataques
