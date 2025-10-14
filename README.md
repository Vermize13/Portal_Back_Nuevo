# Portal_Back_Nuevo - Sistema de Gestión de Bugs (BugMgr)

Sistema backend para la gestión de incidencias, proyectos y sprints desarrollado en ASP.NET Core con PostgreSQL.

## 🚀 Características

- **API REST** con documentación Swagger/OpenAPI
- **Base de datos PostgreSQL** con Entity Framework Core
- **Arquitectura en capas** (Domain, Infrastructure, Repository, API)
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
- **PostgreSQL** - Base de datos
- **Npgsql** - Proveedor PostgreSQL para EF Core
- **Swashbuckle.AspNetCore 9.0.6** - Documentación Swagger/OpenAPI

## 📦 Requisitos Previos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 12+](https://www.postgresql.org/download/)
- IDE recomendado: Visual Studio 2022, Visual Studio Code o JetBrains Rider

## ⚙️ Configuración

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

## 📚 Documentación Swagger

La API incluye documentación interactiva con Swagger UI disponible en modo Development:

**URL:** `http://localhost:5046/`

![Swagger UI](https://github.com/user-attachments/assets/665a330e-5116-46ea-8ac8-472119dad604)

### Funcionalidades de Swagger

- ✅ Explorar todos los endpoints disponibles
- ✅ Probar endpoints directamente desde el navegador
- ✅ Ver esquemas de datos y modelos
- ✅ Exportar especificación OpenAPI en formato JSON

Para más detalles, consulta [API/README.md](API/README.md)

## 🗄️ Modelo de Datos

El sistema gestiona las siguientes entidades principales:

- **Users** - Usuarios del sistema
- **Roles** - Roles y permisos
- **Projects** - Proyectos
- **Sprints** - Sprints dentro de proyectos
- **Incidents** - Incidencias/bugs
- **Labels** - Etiquetas para clasificación
- **Comments** - Comentarios en incidencias
- **Attachments** - Archivos adjuntos
- **Notifications** - Notificaciones del sistema
- **AuditLogs** - Registro de auditoría
- **Backups/Restores** - Gestión de respaldos

## 🔧 Comandos Útiles

### Compilar la solución
```bash
dotnet build BugMgr.sln
```

### Ejecutar tests (cuando estén disponibles)
```bash
dotnet test
```

### Crear una nueva migración
```bash
cd Infrastructure
dotnet ef migrations add NombreDeLaMigracion
```

### Aplicar migraciones
```bash
cd Infrastructure
dotnet ef database update
```

### Ejecutar la API
```bash
cd API
dotnet run
```

## 🤝 Contribuir

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📝 Licencia

Este proyecto es privado y pertenece al equipo de desarrollo.

## 📞 Contacto

Portal_Back_Nuevo Team - [@Vermize13](https://github.com/Vermize13)

---

**Nota:** Este proyecto implementa los requerimientos especificados en `Requerimientos.md` siguiendo las mejores prácticas de desarrollo en .NET.
