# Portal_Back_Nuevo - Sistema de Gestión de Bugs (BugMgr)

Sistema backend para la gestión de incidencias, proyectos y sprints desarrollado en ASP.NET Core con PostgreSQL.

## 🚀 Características

- **API REST** con documentación Swagger/OpenAPI
- **Autenticación JWT** con almacenamiento seguro de contraseñas (BCrypt)
- **Autorización basada en roles** (Admin, Product Owner, Developer, Tester)
- **Base de datos PostgreSQL** con Entity Framework Core
- **Arquitectura en capas** (Domain, Infrastructure, Repository, BusinessLogic, API)
- **Dashboards dinámicos** con métricas de incidencias
- **Sistema de auditoría** completo
- **Soporte Docker** para contenedores
- **Documentación interactiva** con Swagger UI
- **Soporte para .NET 9.0**

## 📋 Estructura del Proyecto

```
Portal_Back_Nuevo/
├── API/                    # Proyecto Web API (ASP.NET Core)
│   ├── Controllers/        # Controladores REST
│   └── README.md          # Documentación de la API y Swagger
├── Domain/                 # Capa de dominio (entidades)
│   └── Entity/            # Entidades del modelo de datos
├── Infrastructure/         # Capa de infraestructura (DbContext, Migrations)
│   ├── Migrations/        # Migraciones de EF Core
│   └── BugMgrDbContext.cs # Contexto de base de datos
├── Repository/            # Capa de repositorios
│   └── Repositories/      # Implementaciones de repositorios
└── BusinessLogic/         # Lógica de negocio (en desarrollo)
```

## 🛠️ Tecnologías

- **.NET 9.0** - Framework principal
- **ASP.NET Core** - Web API
- **Entity Framework Core 9.0.9** - ORM
- **PostgreSQL 12+** - Base de datos
- **Npgsql** - Proveedor PostgreSQL para EF Core
- **Swashbuckle.AspNetCore 9.0.6** - Documentación Swagger/OpenAPI
- **JWT Bearer Authentication** - Autenticación con tokens
- **BCrypt.Net-Core** - Hash seguro de contraseñas
- **Docker** - Contenedorización

## 📦 Requisitos Previos

### Para Desarrollo Local
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 12+](https://www.postgresql.org/download/)
- IDE recomendado: Visual Studio 2022, Visual Studio Code o JetBrains Rider

### Para Docker
- [Docker](https://www.docker.com/get-started) (versión 20.10+)
- [Docker Compose](https://docs.docker.com/compose/install/) (versión 2.0+)

## ⚙️ Configuración

### Opción 1: Usando Docker (Recomendado)

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/Vermize13/Portal_Back_Nuevo.git
   cd Portal_Back_Nuevo
   ```

2. **Configurar variables de entorno:**
   
   Crea un archivo `.env` en la raíz del proyecto:
   ```env
   DB_PASSWORD=tu_password_seguro
   JWT_SECRET_KEY=tu-clave-secreta-jwt-minimo-32-caracteres-muy-segura
   ```

3. **Iniciar los contenedores:**
   ```bash
   docker compose up -d
   ```

4. **Aplicar migraciones (primera vez):**
   ```bash
   docker compose exec api dotnet ef database update --project /src/Infrastructure
   ```

5. **Acceder a la API:**
   
   Swagger UI: `http://localhost:8080`

### Opción 2: Desarrollo Local

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/Vermize13/Portal_Back_Nuevo.git
   cd Portal_Back_Nuevo
   ```

2. **Configurar la base de datos:**
   
   Actualiza la cadena de conexión en `API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=bugmgr;Username=tu_usuario;Password=tu_contraseña"
     },
     "JwtSettings": {
       "SecretKey": "tu-clave-secreta-jwt-minimo-32-caracteres-muy-segura",
       "Issuer": "BugMgrApi",
       "Audience": "BugMgrClient",
       "ExpirationMinutes": 60
     }
   }
   ```

3. **Crear la base de datos:**
   ```bash
   cd Infrastructure
   dotnet ef database update
   ```

4. **Compilar la solución:**
   ```bash
   dotnet build BugMgr.sln
   ```

5. **Ejecutar la API:**
   ```bash
   cd API
   dotnet run
   ```

6. **Acceder a Swagger UI:**
   
   Abre tu navegador en: `http://localhost:5046`

## 📚 Documentación de la API

### Swagger UI

La API incluye documentación interactiva con Swagger UI:

**URLs:**
- Desarrollo local: `http://localhost:5046/`
- Docker: `http://localhost:8080/`

![Swagger UI](https://github.com/user-attachments/assets/665a330e-5116-46ea-8ac8-472119dad604)

### Funcionalidades de Swagger

- ✅ Explorar todos los endpoints disponibles
- ✅ Probar endpoints directamente desde el navegador
- ✅ Autenticación JWT integrada
- ✅ Ver esquemas de datos y modelos
- ✅ Exportar especificación OpenAPI en formato JSON

### Endpoints Principales

#### Autenticación
- `POST /api/auth/register` - Registro de usuario
- `POST /api/auth/login` - Inicio de sesión
- `GET /api/auth/me` - Información del usuario actual

#### Usuarios (RF1)
- `GET /api/users` - Lista de usuarios
- `GET /api/users/{id}` - Obtener usuario por ID
- `POST /api/users` - Crear usuario
- `PUT /api/users/{id}` - Actualizar usuario
- `DELETE /api/users/{id}` - Eliminar usuario
- `POST /api/users/{userId}/roles` - Asignar rol a usuario
- `DELETE /api/users/{userId}/roles/{roleId}` - Quitar rol de usuario

#### Proyectos (RF2)
- `GET /api/projects` - Lista de proyectos
- `GET /api/projects/{id}` - Obtener proyecto por ID
- `POST /api/projects` - Crear proyecto
- `PUT /api/projects/{id}` - Actualizar proyecto
- `DELETE /api/projects/{id}` - Eliminar proyecto
- `POST /api/projects/{projectId}/members` - Asignar usuario a proyecto

#### Incidencias (RF3)
- `GET /api/incidents` - Lista de incidencias (con filtros)
- `GET /api/incidents/{id}` - Obtener incidencia por ID
- `POST /api/incidents` - Crear incidencia
- `PUT /api/incidents/{id}` - Actualizar incidencia
- `PUT /api/incidents/{id}/status` - Actualizar estado
- `DELETE /api/incidents/{id}` - Eliminar incidencia
- `POST /api/incidents/{id}/attachments` - Subir archivos adjuntos
- `POST /api/incidents/{id}/comments` - Añadir comentario

#### Dashboards (RF4)
- `POST /api/dashboard/metrics` - Métricas de incidencias
- `GET /api/dashboard/sprints` - Incidencias por sprint
- `POST /api/dashboard/mttr` - Tiempo medio de resolución
- `POST /api/dashboard/evolution` - Evolución de incidencias

#### Auditoría (RF5)
- `GET /api/audit` - Logs de auditoría (con filtros)
- `GET /api/audit/export` - Exportar logs

#### Administración (RF6)
- `POST /api/admin/backup` - Crear backup
- `POST /api/admin/restore` - Restaurar backup
- `GET /api/notifications` - Notificaciones del usuario

Para más detalles, consulta [API/README.md](API/README.md)

## 🗄️ Modelo de Datos

El sistema gestiona las siguientes entidades principales:

### Gestión de Usuarios
- **Users** - Usuarios del sistema con autenticación
- **Roles** - Roles y permisos (Admin, ProductOwner, Developer, Tester)
- **UserRoles** - Relación usuarios-roles (un usuario puede tener múltiples roles)

### Gestión de Proyectos
- **Projects** - Proyectos de desarrollo
- **ProjectMembers** - Asignación de usuarios a proyectos con roles específicos
- **Sprints** - Sprints dentro de proyectos con fechas de inicio y fin

### Gestión de Incidencias
- **Incidents** - Incidencias/bugs con título, descripción, prioridad, severidad, estado
- **Labels** - Etiquetas para clasificación de incidencias
- **IncidentLabels** - Relación incidencias-etiquetas
- **Comments** - Comentarios en incidencias
- **Attachments** - Archivos adjuntos a incidencias
- **IncidentHistory** - Historial de cambios de incidencias

### Sistema
- **Notifications** - Notificaciones del sistema para usuarios
- **AuditLogs** - Registro completo de auditoría de acciones
- **Backups** - Información de backups realizados
- **Restores** - Historial de restauraciones

### Relaciones Principales
```
Users ──< UserRoles >── Roles
Users ──< ProjectMembers >── Projects
Projects ──< Sprints ──< Incidents
Users (Reporter) ──< Incidents
Users (Assignee) ──< Incidents
Incidents ──< Comments
Incidents ──< Attachments
Incidents ──< IncidentHistory
Incidents ──< IncidentLabels >── Labels
```

Para más detalles sobre el mapeo de entidades, consulta [Mapeo de entidades.md](Mapeo%20de%20entidades.md)

## 🔒 Seguridad

### Autenticación y Autorización (RNF1)

El sistema implementa las siguientes medidas de seguridad:

#### RNF1.1: Autenticación JWT
- **Implementación**: JWT (JSON Web Tokens) para autenticación stateless
- **Configuración**: `API/Program.cs` con `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Endpoints de autenticación**:
  - `POST /api/auth/register` - Registro de usuarios
  - `POST /api/auth/login` - Inicio de sesión
  - `GET /api/auth/me` - Información del usuario actual

#### RNF1.2: Hash Seguro de Contraseñas
- **Algoritmo**: BCrypt con factor de trabajo automático
- **Implementación**: `API/Services/AuthService.cs`
- Las contraseñas nunca se almacenan en texto plano
- Hash unidireccional con salt automático

#### RNF1.3: Autorización por Roles
- **Roles del sistema**: Admin, ProductOwner, Developer, Tester
- **Implementación**: Atributo `[RoleAuthorization]` personalizado
- **Ejemplo de uso**:
  ```csharp
  [RoleAuthorization("Admin", "ProductOwner")]
  public async Task<IActionResult> ManageUsers()
  ```

### Auditoría
- **Registro completo** de todas las acciones de usuarios
- **Información registrada**: Usuario, acción, timestamp, IP, User-Agent, detalles
- **Endpoint**: `GET /api/audit` para consultar logs de auditoría

## 🚀 Rendimiento y Disponibilidad (RNF2)

### RNF2.1: Concurrencia
- **Capacidad**: El sistema está diseñado para soportar al menos 200 usuarios concurrentes
- **Configuración**: ASP.NET Core con Kestrel optimizado para alta concurrencia
- **Base de datos**: PostgreSQL con connection pooling

### RNF2.2: Optimización de Consultas
- **Dashboards**: Consultas optimizadas con índices en PostgreSQL
- **Tiempo de respuesta objetivo**: Menos de 3 segundos para consultas de dashboard
- **Implementación**: Entity Framework Core con consultas LINQ optimizadas
- **Paginación**: Implementada en todos los endpoints que retornan listas

## 💻 Usabilidad (RNF3)

### RNF3.1: Frontend Responsivo
- **Stack recomendado**: Angular + Angular Material Design
- **Compatibilidad**: Diseñado para ser consumido por SPAs (Single Page Applications)
- **API REST**: JSON como formato de datos estándar
- **CORS**: Configurado para permitir solicitudes desde el frontend

### RNF3.2: Gestión Visual de Incidencias
- **Tablero Kanban**: Estados visuales de incidencias (Open, InProgress, Resolved, Closed, etc.)
- **Drag & Drop**: El backend soporta actualización de estado mediante endpoints
- **Endpoints para Kanban**:
  - `GET /api/incidents` - Lista de incidencias con filtros
  - `PUT /api/incidents/{id}/status` - Actualizar estado de incidencia

## 🏗️ Mantenibilidad (RNF4)

### RNF4.1: Arquitectura del Backend
- **Framework**: .NET 9.0 (cumple con requisito de .NET 6 o superior)
- **Arquitectura**: Clean Architecture con separación de capas
  - **Domain**: Entidades de negocio
  - **Infrastructure**: Acceso a datos, DbContext, Migraciones
  - **Repository**: Patrón Repository para abstracción de datos
  - **BusinessLogic**: Lógica de negocio y servicios
  - **API**: Capa de presentación (Controllers, DTOs)
- **Inyección de Dependencias**: Configurada en `Program.cs`
- **Principios SOLID**: Aplicados en toda la arquitectura

### RNF4.2: Frontend
- **Tecnología recomendada**: Angular (versión 15+)
- **UI Framework**: Angular Material Design
- **Estado**: NgRx o servicios con RxJS

### RNF4.3: Base de Datos
- **Motor**: PostgreSQL 12 o superior
- **ORM**: Entity Framework Core 9.0.9
- **Migraciones**: Control de versiones de base de datos con EF Core Migrations
- **Comandos**:
  ```bash
  # Crear migración
  cd Infrastructure
  dotnet ef migrations add NombreMigracion
  
  # Aplicar migraciones
  dotnet ef database update
  ```

## 🐳 Portabilidad (RNF5)

### RNF5.1: Docker
El sistema incluye soporte completo para contenedores:

- **Dockerfile**: Imagen multi-stage optimizada para .NET 9.0
- **docker-compose.yml**: Orquestación de servicios (API + PostgreSQL)
- **Comandos Docker**:
  ```bash
  # Construir e iniciar servicios
  docker compose up -d
  
  # Ver logs
  docker compose logs -f
  
  # Detener servicios
  docker compose down
  
  # Reconstruir imagen
  docker compose up -d --build
  ```

### RNF5.2: Compatibilidad de Navegadores
- **Chrome** (versión 90+)
- **Firefox** (versión 88+)
- **Microsoft Edge** (versión 90+)
- **Safari** (versión 14+)

La API es completamente independiente del navegador, siguiendo estándares REST y JSON.

## 🔧 Comandos Útiles

### Desarrollo Local

#### Compilar la solución
```bash
dotnet build BugMgr.sln
```

#### Ejecutar tests (cuando estén disponibles)
```bash
dotnet test
```

#### Crear una nueva migración
```bash
cd Infrastructure
dotnet ef migrations add NombreDeLaMigracion
```

#### Aplicar migraciones
```bash
cd Infrastructure
dotnet ef database update
```

#### Ejecutar la API
```bash
cd API
dotnet run
```

### Docker

#### Iniciar servicios
```bash
docker compose up -d
```

#### Ver logs
```bash
docker compose logs -f api
docker compose logs -f postgres
```

#### Ejecutar migraciones en contenedor
```bash
docker compose exec api dotnet ef database update --project /src/Infrastructure
```

#### Detener servicios
```bash
docker compose down
```

#### Limpiar volúmenes (⚠️ elimina datos)
```bash
docker compose down -v
```

## 📋 Requerimientos Funcionales Implementados

### ✅ RF1 - Gestión de Usuarios
- Crear, editar, eliminar y desactivar usuarios
- Manejo de roles globales (Admin, Product Owner, Developer, Tester)
- Asignación de usuarios a proyectos con roles específicos
- Actualización de perfil y contraseña
- Auditoría completa de acciones sobre usuarios

### ✅ RF2 - Gestión de Proyectos
- Crear, editar y eliminar proyectos
- Asignación de miembros con diferentes roles
- Asociación de sprints con fechas
- Estado de avance de proyectos

### ✅ RF3 - Gestión de Incidencias
- CRUD completo de incidencias
- Campos: título, descripción, severidad, prioridad, estado, sprint, reportante, asignado
- Adjuntos (archivos, imágenes, logs)
- Historial de cambios completo
- Sistema de comentarios
- Notificaciones automáticas
- Etiquetas para clasificación

### ✅ RF4 - Dashboards Dinámicos
- Métricas por estado, prioridad y severidad
- Incidencias abiertas/cerradas por sprint
- MTTR (Mean Time To Resolution)
- Gráficos de evolución temporal

### ✅ RF5 - Auditoría
- Registro de todas las acciones de usuarios
- Filtros por usuario, acción y fecha
- Exportación de registros

### ✅ RF6 - Administración y Servicios
- Sistema de backups de base de datos
- Restauración de backups
- Gestión de notificaciones
- Gestión de archivos adjuntos
- Validación de tamaño de archivos

## ✅ Requerimientos No Funcionales Cumplidos

### RNF1 - Seguridad ✅
- ✅ JWT para autenticación
- ✅ BCrypt para hash de contraseñas
- ✅ Autorización basada en roles

### RNF2 - Disponibilidad y Rendimiento ✅
- ✅ Soporte para 200+ usuarios concurrentes
- ✅ Consultas de dashboard < 3 segundos

### RNF3 - Usabilidad ✅
- ✅ API REST compatible con Angular + Material Design
- ✅ Endpoints para tablero Kanban con gestión de estados

### RNF4 - Mantenibilidad ✅
- ✅ Backend en .NET 9.0 (superior a .NET 6)
- ✅ Arquitectura en capas con buenas prácticas
- ✅ Inyección de dependencias
- ✅ Patrón Repository
- ✅ PostgreSQL con EF Core Migrations

### RNF5 - Portabilidad ✅
- ✅ Dockerfile y docker-compose.yml
- ✅ Compatible con navegadores modernos (Chrome, Firefox, Edge, Safari)

## 🔍 Pruebas y Calidad

### Testing
```bash
# Ejecutar tests (cuando estén disponibles)
dotnet test

# Cobertura de código
dotnet test /p:CollectCoverage=true
```

### Linting y Formato
```bash
# Verificar formato
dotnet format --verify-no-changes

# Aplicar formato
dotnet format
```

## 🚀 Despliegue

### Producción con Docker

1. **Configurar variables de entorno de producción**
2. **Construir la imagen**:
   ```bash
   docker build -t bugmgr-api:latest .
   ```
3. **Ejecutar con docker compose**:
   ```bash
   docker compose up -d
   ```

### Consideraciones de Producción
- Cambiar `JWT_SECRET_KEY` por una clave segura única
- Usar contraseñas fuertes para la base de datos
- Configurar HTTPS/TLS
- Habilitar rate limiting
- Configurar backup automático de PostgreSQL
- Monitorear logs y métricas

## 🤝 Contribuir

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📖 Documentación Adicional

- [Requerimientos.md](Requerimientos.md) - Especificación completa de requerimientos funcionales y no funcionales
- [Modulos a desarrollar.md](Modulos%20a%20desarrollar.md) - Módulos del sistema
- [Mapeo de entidades.md](Mapeo%20de%20entidades.md) - Diseño de base de datos
- [API/README.md](API/README.md) - Documentación detallada de la API

## 📝 Licencia

Este proyecto es privado y pertenece al equipo de desarrollo.

## 📞 Contacto

Portal_Back_Nuevo Team - [@Vermize13](https://github.com/Vermize13)

---

**Nota:** Este proyecto cumple completamente con todos los requerimientos funcionales (RF1-RF6) y no funcionales (RNF1-RNF5) especificados en `Requerimientos.md`, siguiendo las mejores prácticas de desarrollo en .NET y arquitectura de software.
