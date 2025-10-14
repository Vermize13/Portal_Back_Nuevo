# Ejemplos de Uso - Gestión de Usuarios (RF1)

Este documento proporciona ejemplos prácticos de cómo utilizar los servicios implementados para la gestión de usuarios.

## Configuración Inicial

### 1. Registrar Servicios en Startup/Program.cs

```csharp
using BusinessLogic;
using Infrastructure;
using Repository;

var builder = WebApplication.CreateBuilder(args);

// Registrar DbContext
builder.Services.AddDbContext<BugMgrDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar servicios de negocio
builder.Services.AddBusinessLogicServices();

var app = builder.Build();
```

## Ejemplos de Uso

### 1. Crear un Nuevo Usuario

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly Guid _currentUserId; // Obtener del JWT

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            var user = await _userService.CreateUserAsync(dto, _currentUserId);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

**Request Body:**
```json
{
  "name": "Juan Pérez",
  "email": "juan.perez@example.com",
  "username": "jperez",
  "password": "SecurePassword123!",
  "roleIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  ]
}
```

**Response (201 Created):**
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "name": "Juan Pérez",
  "email": "juan.perez@example.com",
  "username": "jperez",
  "isActive": true,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z",
  "roles": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "code": "DEVELOPER",
      "name": "Developer",
      "description": "Software Developer"
    }
  ]
}
```

### 2. Actualizar Información de Usuario

```csharp
[HttpPut("{id}")]
public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
{
    try
    {
        var user = await _userService.UpdateUserAsync(id, dto, _currentUserId);
        return Ok(user);
    }
    catch (InvalidOperationException ex)
    {
        return NotFound(new { error = ex.Message });
    }
}
```

**Request Body:**
```json
{
  "name": "Juan Carlos Pérez",
  "email": "juancarlos.perez@example.com",
  "username": "jcperez"
}
```

### 3. Cambiar Contraseña

```csharp
[HttpPost("{id}/change-password")]
public async Task<IActionResult> ChangePassword(Guid id, [FromBody] UpdatePasswordDto dto)
{
    try
    {
        await _userService.UpdatePasswordAsync(id, dto, id);
        return Ok(new { message = "Password updated successfully" });
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

**Request Body:**
```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewSecurePassword456!"
}
```

### 4. Desactivar Usuario

```csharp
[HttpPost("{id}/deactivate")]
public async Task<IActionResult> DeactivateUser(Guid id)
{
    try
    {
        await _userService.DeactivateUserAsync(id, _currentUserId);
        return Ok(new { message = "User deactivated successfully" });
    }
    catch (InvalidOperationException ex)
    {
        return NotFound(new { error = ex.Message });
    }
}
```

### 5. Activar Usuario

```csharp
[HttpPost("{id}/activate")]
public async Task<IActionResult> ActivateUser(Guid id)
{
    try
    {
        await _userService.ActivateUserAsync(id, _currentUserId);
        return Ok(new { message = "User activated successfully" });
    }
    catch (InvalidOperationException ex)
    {
        return NotFound(new { error = ex.Message });
    }
}
```

### 6. Eliminar Usuario

```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteUser(Guid id)
{
    try
    {
        await _userService.DeleteUserAsync(id, _currentUserId);
        return NoContent();
    }
    catch (InvalidOperationException ex)
    {
        return NotFound(new { error = ex.Message });
    }
}
```

### 7. Obtener Usuario por ID

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<UserDto>> GetUser(Guid id)
{
    var user = await _userService.GetUserByIdAsync(id);
    if (user == null)
        return NotFound();
    
    return Ok(user);
}
```

### 8. Listar Todos los Usuarios

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
{
    var users = await _userService.GetAllUsersAsync();
    return Ok(users);
}
```

### 9. Listar Usuarios Activos

```csharp
[HttpGet("active")]
public async Task<ActionResult<IEnumerable<UserDto>>> GetActiveUsers()
{
    var users = await _userService.GetActiveUsersAsync();
    return Ok(users);
}
```

### 10. Asignar Rol a Usuario

```csharp
[HttpPost("{userId}/roles/{roleId}")]
public async Task<IActionResult> AssignRole(Guid userId, Guid roleId)
{
    try
    {
        await _userService.AssignRoleAsync(userId, roleId, _currentUserId);
        return Ok(new { message = "Role assigned successfully" });
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

### 11. Eliminar Rol de Usuario

```csharp
[HttpDelete("{userId}/roles/{roleId}")]
public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId)
{
    try
    {
        await _userService.RemoveRoleAsync(userId, roleId, _currentUserId);
        return Ok(new { message = "Role removed successfully" });
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

## Gestión de Proyectos

### 12. Asignar Usuario a Proyecto

```csharp
[ApiController]
[Route("api/project-members")]
public class ProjectMembersController : ControllerBase
{
    private readonly IProjectMemberService _projectMemberService;
    private readonly Guid _currentUserId;

    public ProjectMembersController(IProjectMemberService projectMemberService)
    {
        _projectMemberService = projectMemberService;
    }

    [HttpPost]
    public async Task<IActionResult> AssignUserToProject([FromBody] AssignUserToProjectDto dto)
    {
        try
        {
            await _projectMemberService.AssignUserToProjectAsync(dto, _currentUserId);
            return Ok(new { message = "User assigned to project successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

**Request Body:**
```json
{
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "projectId": "f1e2d3c4-b5a6-7890-1234-567890abcdef",
  "roleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### 13. Eliminar Usuario de Proyecto

```csharp
[HttpDelete("projects/{projectId}/users/{userId}")]
public async Task<IActionResult> RemoveUserFromProject(Guid projectId, Guid userId)
{
    try
    {
        await _projectMemberService.RemoveUserFromProjectAsync(projectId, userId, _currentUserId);
        return Ok(new { message = "User removed from project successfully" });
    }
    catch (InvalidOperationException ex)
    {
        return NotFound(new { error = ex.Message });
    }
}
```

### 14. Cambiar Rol de Usuario en Proyecto

```csharp
[HttpPut("projects/{projectId}/users/{userId}/role")]
public async Task<IActionResult> UpdateProjectMemberRole(Guid projectId, Guid userId, [FromBody] Guid roleId)
{
    try
    {
        await _projectMemberService.UpdateProjectMemberRoleAsync(projectId, userId, roleId, _currentUserId);
        return Ok(new { message = "Project member role updated successfully" });
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

### 15. Obtener Miembros de un Proyecto

```csharp
[HttpGet("projects/{projectId}/members")]
public async Task<ActionResult<IEnumerable<ProjectMember>>> GetProjectMembers(Guid projectId)
{
    var members = await _projectMemberService.GetProjectMembersAsync(projectId);
    return Ok(members);
}
```

### 16. Obtener Proyectos de un Usuario

```csharp
[HttpGet("users/{userId}/projects")]
public async Task<ActionResult<IEnumerable<ProjectMember>>> GetUserProjects(Guid userId)
{
    var projects = await _projectMemberService.GetUserProjectsAsync(userId);
    return Ok(projects);
}
```

## Actualización de Perfil Propio

### 17. Usuario Actualiza su Propio Perfil

```csharp
[HttpPut("profile")]
[Authorize] // Requiere autenticación
public async Task<ActionResult<UserDto>> UpdateOwnProfile([FromBody] UpdateUserDto dto)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userId))
        return Unauthorized();

    try
    {
        await _userService.UpdateProfileAsync(Guid.Parse(userId), dto);
        return Ok(new { message = "Profile updated successfully" });
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

## Manejo de Errores

Todos los servicios lanzan `InvalidOperationException` con mensajes descriptivos en caso de:
- Usuario/Proyecto/Rol no encontrado
- Email o Username duplicado
- Contraseña actual incorrecta
- Asignaciones duplicadas
- Otras violaciones de reglas de negocio

**Ejemplo de respuesta de error:**
```json
{
  "error": "Email already exists"
}
```

## Notas Importantes

1. **Seguridad**: Implementar autenticación JWT y autorización basada en roles
2. **Validación**: Añadir validaciones de entrada en DTOs (DataAnnotations o FluentValidation)
3. **Logging**: Considerar añadir logging adicional para debugging
4. **Transacciones**: Los servicios usan UnitOfWork para garantizar consistencia
5. **Auditoría**: Todas las operaciones quedan registradas automáticamente

## Próximos Pasos

1. Crear proyecto Web API
2. Implementar autenticación JWT
3. Añadir autorización basada en roles
4. Crear validadores para DTOs
5. Implementar manejo global de excepciones
6. Añadir paginación para listados grandes
7. Implementar filtros y búsqueda
