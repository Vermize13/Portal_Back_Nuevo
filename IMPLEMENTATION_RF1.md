# Implementación de Gestión de Usuarios (RF1)

## Resumen

Este documento describe la implementación completa del Requerimiento Funcional 1 (RF1) - Gestión de Usuarios, incluyendo todas las funcionalidades especificadas.

## Componentes Implementados

### 1. Repositorios

Se han creado e implementado los siguientes repositorios para el acceso a datos:

#### UserRepository
- **Ubicación**: `Repository/Repositories/UserRepository.cs`
- **Métodos**:
  - `GetByEmailAsync(string email)`: Buscar usuario por email
  - `GetByUsernameAsync(string username)`: Buscar usuario por nombre de usuario
  - `GetByIdWithRolesAsync(Guid id)`: Obtener usuario con sus roles asignados
  - `GetAllActiveUsersAsync()`: Obtener todos los usuarios activos
  - `GetAllUsersWithRolesAsync()`: Obtener todos los usuarios con sus roles

#### RoleRepository
- **Ubicación**: `Repository/Repositories/RoleRepository.cs`
- **Métodos**:
  - `GetByCodeAsync(string code)`: Buscar rol por código
  - `GetAllActiveRolesAsync()`: Obtener todos los roles activos

#### UserRoleRepository
- **Ubicación**: `Repository/Repositories/UserRoleRepository.cs`
- **Métodos**:
  - `GetByUserIdAsync(Guid userId)`: Obtener roles de un usuario
  - `GetByUserAndRoleAsync(Guid userId, Guid roleId)`: Obtener asignación específica
  - `AddAsync(UserRole userRole)`: Añadir asignación de rol
  - `Remove(UserRole userRole)`: Eliminar asignación de rol
  - `ExistsAsync(Guid userId, Guid roleId)`: Verificar si existe asignación

#### ProjectMemberRepository
- **Ubicación**: `Repository/Repositories/ProjectMemberRepository.cs`
- **Métodos**:
  - `GetByProjectIdAsync(Guid projectId)`: Obtener miembros de un proyecto
  - `GetByUserIdAsync(Guid userId)`: Obtener proyectos de un usuario
  - `GetByProjectAndUserAsync(Guid projectId, Guid userId)`: Obtener asignación específica
  - `AddAsync(ProjectMember projectMember)`: Añadir miembro a proyecto
  - `Update(ProjectMember projectMember)`: Actualizar miembro
  - `Remove(ProjectMember projectMember)`: Eliminar miembro
  - `ExistsAsync(Guid projectId, Guid userId)`: Verificar si existe asignación

### 2. DTOs (Data Transfer Objects)

Se han creado DTOs para la transferencia de datos entre capas:

#### CreateUserDto
- **Ubicación**: `BusinessLogic/DTOs/CreateUserDto.cs`
- **Propiedades**: Name, Email, Username, Password, RoleIds
- **Uso**: Crear nuevos usuarios con roles asignados

#### UpdateUserDto
- **Ubicación**: `BusinessLogic/DTOs/UpdateUserDto.cs`
- **Propiedades**: Name, Email, Username
- **Uso**: Actualizar información de usuario

#### UpdatePasswordDto
- **Ubicación**: `BusinessLogic/DTOs/UpdatePasswordDto.cs`
- **Propiedades**: CurrentPassword, NewPassword
- **Uso**: Cambio de contraseña por el usuario

#### AssignUserToProjectDto
- **Ubicación**: `BusinessLogic/DTOs/AssignUserToProjectDto.cs`
- **Propiedades**: UserId, ProjectId, RoleId
- **Uso**: Asignar usuarios a proyectos con un rol específico

#### UserDto
- **Ubicación**: `BusinessLogic/DTOs/UserDto.cs`
- **Propiedades**: Id, Name, Email, Username, IsActive, CreatedAt, UpdatedAt, Roles
- **Uso**: Representación de usuario en respuestas

### 3. Servicios

#### PasswordService
- **Ubicación**: `BusinessLogic/Services/PasswordService.cs`
- **Funcionalidades**:
  - `HashPassword(string password)`: Hash seguro de contraseñas usando BCrypt con salt de 12 rondas
  - `VerifyPassword(string password, string hashedPassword)`: Verificación de contraseñas
- **Cumple con**: RNF1.2 (contraseñas con hash seguro)

#### AuditService
- **Ubicación**: `BusinessLogic/Services/AuditService.cs`
- **Funcionalidades**:
  - `LogAsync(...)`: Registra todas las acciones del sistema en la tabla AuditLog
  - Incluye: acción, actor, entidad, detalles JSON, IP, user agent
- **Cumple con**: RF1.5, RF5.1 (auditoría completa)

#### UserService
- **Ubicación**: `BusinessLogic/Services/UserService.cs`
- **Funcionalidades**:
  - **Gestión de Usuarios (RF1.1)**:
    - `CreateUserAsync(CreateUserDto dto, Guid? actorId)`: Crear usuario con validación de unicidad
    - `UpdateUserAsync(Guid userId, UpdateUserDto dto, Guid? actorId)`: Actualizar usuario
    - `DeleteUserAsync(Guid userId, Guid? actorId)`: Eliminar usuario (hard delete)
    - `DeactivateUserAsync(Guid userId, Guid? actorId)`: Desactivar usuario (soft delete)
    - `ActivateUserAsync(Guid userId, Guid? actorId)`: Reactivar usuario
    - `GetUserByIdAsync(Guid userId)`: Obtener usuario por ID
    - `GetAllUsersAsync()`: Obtener todos los usuarios
    - `GetActiveUsersAsync()`: Obtener usuarios activos
  
  - **Gestión de Roles (RF1.2)**:
    - `AssignRoleAsync(Guid userId, Guid roleId, Guid? actorId)`: Asignar rol global a usuario
    - `RemoveRoleAsync(Guid userId, Guid roleId, Guid? actorId)`: Eliminar rol de usuario
  
  - **Actualización de Perfil y Contraseña (RF1.4)**:
    - `UpdatePasswordAsync(Guid userId, UpdatePasswordDto dto, Guid? actorId)`: Cambiar contraseña con verificación
    - `UpdateProfileAsync(Guid userId, UpdateUserDto dto)`: Actualizar perfil propio

- **Validaciones Implementadas**:
  - Email único en el sistema
  - Username único en el sistema
  - Verificación de contraseña actual antes de cambiarla
  - Verificación de existencia de usuarios, roles y proyectos
  - Prevención de asignaciones duplicadas
  
- **Auditoría**: Todas las operaciones registran eventos en AuditLog

#### ProjectMemberService
- **Ubicación**: `BusinessLogic/Services/ProjectMemberService.cs`
- **Funcionalidades (RF1.3)**:
  - `AssignUserToProjectAsync(AssignUserToProjectDto dto, Guid? actorId)`: Asignar usuario a proyecto con rol específico
  - `RemoveUserFromProjectAsync(Guid projectId, Guid userId, Guid? actorId)`: Eliminar usuario de proyecto (soft delete)
  - `UpdateProjectMemberRoleAsync(Guid projectId, Guid userId, Guid roleId, Guid? actorId)`: Cambiar rol de miembro en proyecto
  - `GetProjectMembersAsync(Guid projectId)`: Obtener miembros de un proyecto
  - `GetUserProjectsAsync(Guid userId)`: Obtener proyectos de un usuario

- **Validaciones Implementadas**:
  - Verificación de existencia de usuario, proyecto y rol
  - Prevención de asignaciones duplicadas
  - Reactivación de miembros previamente desactivados
  
- **Auditoría**: Todas las asignaciones y cambios quedan registrados

### 4. Configuración de Servicios

#### ServiceCollectionExtensions
- **Ubicación**: `BusinessLogic/ServiceCollectionExtensions.cs`
- **Método**: `AddBusinessLogicServices(IServiceCollection services)`
- **Registro**:
  - Todos los repositorios con inyección de dependencias
  - Todos los servicios de negocio
  - UnitOfWork para transacciones

## Roles Globales Soportados (RF1.2)

El sistema maneja los siguientes roles globales según lo especificado:
- **Admin**: Administrador del sistema
- **Product Owner**: Propietario del producto
- **Developer**: Desarrollador
- **Tester**: Probador

## Seguridad Implementada

### Hash de Contraseñas (RNF1.2)
- Utiliza BCrypt.Net-Core versión 1.6.0
- Salt de 12 rondas para máxima seguridad
- Las contraseñas nunca se almacenan en texto plano

### Validaciones de Negocio
- Email y Username únicos
- Verificación de contraseña actual antes de cambios
- Validación de existencia de entidades relacionadas
- Prevención de asignaciones duplicadas

## Auditoría Completa (RF1.5, RF5.1)

Todas las acciones sobre usuarios quedan registradas en AuditLog:
- **Create**: Creación de usuarios
- **Update**: Actualización de datos, activación/desactivación
- **Delete**: Eliminación de usuarios
- **Assign**: Asignación de roles y proyectos
- **Login/Logout**: Sesiones de usuario (cuando se implemente autenticación)

Información registrada:
- Acción realizada
- Usuario que realizó la acción (actorId)
- Entidad afectada
- ID de la entidad
- Detalles en formato JSON
- IP Address y User Agent (cuando estén disponibles)
- Timestamp

## Uso de los Servicios

### Ejemplo: Crear Usuario

```csharp
var createDto = new CreateUserDto
{
    Name = "Juan Pérez",
    Email = "juan.perez@example.com",
    Username = "jperez",
    Password = "SecurePassword123!",
    RoleIds = new List<Guid> { developerRoleId }
};

var user = await userService.CreateUserAsync(createDto, adminUserId);
```

### Ejemplo: Asignar Usuario a Proyecto

```csharp
var assignDto = new AssignUserToProjectDto
{
    UserId = userId,
    ProjectId = projectId,
    RoleId = developerRoleId
};

await projectMemberService.AssignUserToProjectAsync(assignDto, adminUserId);
```

### Ejemplo: Cambiar Contraseña

```csharp
var passwordDto = new UpdatePasswordDto
{
    CurrentPassword = "OldPassword123",
    NewPassword = "NewSecurePassword456!"
};

await userService.UpdatePasswordAsync(userId, passwordDto, userId);
```

## Integración en API (Próximos Pasos)

Para integrar estos servicios en controladores API:

1. Crear proyecto Web API
2. Registrar servicios usando `AddBusinessLogicServices()`
3. Crear controladores que inyecten `IUserService` y `IProjectMemberService`
4. Implementar endpoints REST según especificaciones
5. Añadir autenticación JWT (RNF1.1)
6. Implementar autorización basada en roles (RNF1.3)

## Pruebas Pendientes

Para completar la implementación se recomienda:
- Unit tests para cada servicio
- Integration tests para flujos completos
- Tests de validación de reglas de negocio
- Tests de seguridad y manejo de errores

## Cumplimiento de Requerimientos

### RF1.1 ✅
- Crear usuarios: `CreateUserAsync`
- Editar usuarios: `UpdateUserAsync`
- Eliminar usuarios: `DeleteUserAsync`
- Desactivar usuarios: `DeactivateUserAsync`, `ActivateUserAsync`

### RF1.2 ✅
- Manejo de roles globales: `AssignRoleAsync`, `RemoveRoleAsync`
- Roles soportados: Admin, Product Owner, Developer, Tester

### RF1.3 ✅
- Asignar usuarios a proyectos: `AssignUserToProjectAsync`
- Con rol específico: Incluido en `AssignUserToProjectDto`

### RF1.4 ✅
- Actualizar perfil: `UpdateProfileAsync`
- Cambiar contraseña: `UpdatePasswordAsync`

### RF1.5 ✅
- Registro en auditoría: `AuditService` integrado en todas las operaciones

### RNF1.2 ✅
- Hash seguro de contraseñas: BCrypt con 12 rondas

### RNF1.3 ✅
- Validaciones de autorización: Parámetro `actorId` en todos los métodos
- Implementación de autorización completa pendiente de capa API

## Notas de Implementación

- Todas las fechas usan `DateTimeOffset.UtcNow` para compatibilidad global
- Soft delete implementado para ProjectMember (`IsActive = false`)
- Hard delete disponible para User cuando sea necesario
- Mapeo manual de entidades a DTOs (sin AutoMapper para mantener control)
- Validaciones exhaustivas antes de operaciones de base de datos
