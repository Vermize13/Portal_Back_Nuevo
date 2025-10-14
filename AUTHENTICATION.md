# Flujo de Autenticación y Autorización

## Descripción General

El sistema de autenticación implementa JWT (JSON Web Tokens) para asegurar el acceso a los endpoints de la API. Las contraseñas se almacenan con hash seguro usando BCrypt, y todas las acciones de autenticación quedan registradas en la tabla de auditoría.

## Arquitectura

### Componentes Principales

1. **AuthController** (`WebApi/Controllers/AuthController.cs`)
   - Maneja los endpoints de login, registro y obtención de información del usuario actual
   - Valida las credenciales y genera tokens JWT

2. **AuthService** (`WebApi/Services/AuthService.cs`)
   - Implementa la lógica de negocio para autenticación
   - Genera tokens JWT con claims de usuario y roles
   - Gestiona el hash de contraseñas con BCrypt

3. **AuditService** (`WebApi/Services/AuditService.cs`)
   - Registra todas las acciones de autenticación en la base de datos
   - Almacena detalles como IP, User-Agent, y datos específicos de cada acción

4. **RoleAuthorizationAttribute** (`WebApi/Middleware/RoleAuthorizationAttribute.cs`)
   - Atributo personalizado para restringir acceso por roles
   - Uso: `[RoleAuthorization("Admin", "ProductOwner")]`

## Flujo de Autenticación

### 1. Registro de Usuario

**Endpoint:** `POST /api/auth/register`

**Request Body:**
```json
{
  "name": "Juan Pérez",
  "email": "juan.perez@example.com",
  "username": "jperez",
  "password": "password123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "jperez",
  "email": "juan.perez@example.com",
  "roles": ["Developer"],
  "expiresAt": "2025-10-14T01:34:23.978Z"
}
```

**Proceso:**
1. Valida que el username y email no existan
2. Hashea la contraseña usando BCrypt
3. Crea el usuario en la base de datos
4. Asigna el rol "Developer" por defecto
5. Genera un token JWT
6. Registra la acción en auditoría (AuditAction.Create)
7. Retorna el token y datos del usuario

### 2. Login de Usuario

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "username": "jperez",
  "password": "password123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "jperez",
  "email": "juan.perez@example.com",
  "roles": ["Developer", "Admin"],
  "expiresAt": "2025-10-14T01:34:23.978Z"
}
```

**Proceso:**
1. Busca el usuario por username
2. Verifica que el usuario esté activo (IsActive = true)
3. Valida la contraseña usando BCrypt.Verify
4. Carga los roles del usuario desde UserRoles
5. Genera un token JWT con los claims apropiados
6. Registra la acción en auditoría (AuditAction.Login)
7. Retorna el token y datos del usuario

### 3. Acceso a Endpoints Protegidos

**Endpoint:** `GET /api/auth/me`

**Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "jperez",
  "roles": ["Developer", "Admin"]
}
```

**Proceso:**
1. El middleware de autenticación valida el token JWT
2. Extrae los claims del token (userId, username, roles)
3. Retorna la información del usuario autenticado

## Estructura del Token JWT

El token JWT contiene los siguientes claims:

- **NameIdentifier** (sub): ID del usuario (GUID)
- **Name**: Username del usuario
- **Role**: Roles asignados al usuario (múltiples)
- **jti**: ID único del token (GUID)
- **iss**: Issuer (BugMgrApi)
- **aud**: Audience (BugMgrClient)
- **exp**: Timestamp de expiración

## Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=bugmgr;Username=postgres;Password=postgres"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-change-this-in-production-min-32-chars",
    "Issuer": "BugMgrApi",
    "Audience": "BugMgrClient",
    "ExpirationMinutes": 60
  }
}
```

**Importante:** La `SecretKey` debe ser cambiada en producción y debe tener al menos 32 caracteres.

## Roles Disponibles

Según los requerimientos, el sistema soporta los siguientes roles:

1. **Admin**: Administrador del sistema con acceso total
2. **ProductOwner**: Product Owner con permisos de gestión de proyectos
3. **Developer**: Desarrollador con permisos básicos
4. **Tester**: Tester con permisos de gestión de incidencias

## Autorización por Roles

### Uso del Atributo `[RoleAuthorization]`

```csharp
[HttpGet("admin-only")]
[RoleAuthorization("Admin")]
public IActionResult AdminOnlyEndpoint()
{
    return Ok("Solo admins pueden ver esto");
}

[HttpGet("admin-or-po")]
[RoleAuthorization("Admin", "ProductOwner")]
public IActionResult AdminOrPOEndpoint()
{
    return Ok("Admins y Product Owners pueden ver esto");
}
```

### Uso del Atributo `[Authorize]` estándar

```csharp
// Requiere autenticación pero no rol específico
[Authorize]
[HttpGet("protected")]
public IActionResult ProtectedEndpoint()
{
    return Ok("Cualquier usuario autenticado puede ver esto");
}

// Requiere rol específico
[Authorize(Roles = "Admin,ProductOwner")]
[HttpGet("management")]
public IActionResult ManagementEndpoint()
{
    return Ok("Roles específicos requeridos");
}
```

## Seguridad

### Hash de Contraseñas

- Se utiliza **BCrypt** para hashear contraseñas
- BCrypt genera automáticamente un salt único para cada contraseña
- El costo de hashing es el valor por defecto de BCrypt (generalmente 11)
- Las contraseñas nunca se almacenan en texto plano

### Validación de Tokens

- Los tokens se validan en cada request mediante el middleware de autenticación
- Se verifican: firma, issuer, audience, y expiración
- ClockSkew está configurado en cero para mayor precisión en la expiración

### Auditoría

Todas las acciones de autenticación quedan registradas en la tabla `AuditLogs`:

- **Login**: Registra intentos exitosos de login con username, IP y User-Agent
- **Create User**: Registra la creación de nuevos usuarios
- Incluye información contextual en el campo `DetailsJson`

## Extensibilidad

### Agregar Nuevos Roles

1. Insertar el nuevo rol en la tabla `Roles` de la base de datos
2. Asignar el rol a usuarios mediante la tabla `UserRoles`
3. Usar el código del rol en los atributos `[RoleAuthorization]`

### Implementar Recuperación de Contraseña

Para implementar recuperación de contraseña, considerar:

1. Crear un endpoint para solicitar reset (`POST /api/auth/forgot-password`)
2. Generar un token de recuperación temporal
3. Enviar email con link de recuperación
4. Crear endpoint para validar token y establecer nueva contraseña

### Implementar Refresh Tokens

Para mayor seguridad, considerar implementar refresh tokens:

1. Generar un refresh token de larga duración junto con el access token
2. Almacenar refresh tokens en la base de datos
3. Crear endpoint para renovar access token usando refresh token
4. Invalidar refresh tokens al hacer logout

## Testing

### Ejemplos de Uso con cURL

**Registro:**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@example.com",
    "username": "testuser",
    "password": "password123"
  }'
```

**Login:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "password123"
  }'
```

**Acceso Protegido:**
```bash
curl -X GET http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Troubleshooting

### Error: "Invalid username or password"
- Verificar que el usuario existe y está activo (IsActive = true)
- Verificar que la contraseña es correcta

### Error: "Username or email already exists"
- El username o email ya está registrado en el sistema
- Usar credenciales diferentes

### Error: "Unauthorized" (401)
- El token no es válido o ha expirado
- Verificar que el header Authorization esté presente y bien formado
- Obtener un nuevo token mediante login

### Error: "Forbidden" (403)
- El usuario está autenticado pero no tiene el rol requerido
- Verificar los roles asignados al usuario en la base de datos

## Próximos Pasos

- [ ] Implementar throttling para prevenir ataques de fuerza bruta
- [ ] Agregar validación de complejidad de contraseñas
- [ ] Implementar logout con invalidación de tokens
- [ ] Agregar soporte para autenticación de dos factores (2FA)
- [ ] Implementar política de expiración de contraseñas
