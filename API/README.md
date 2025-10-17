# BugMgr API - Documentación

## Descripción

API REST para el sistema de gestión de incidencias BugMgr. Proporciona endpoints para la gestión completa de usuarios, proyectos, sprints, incidencias, dashboards y auditoría.

## Documentación Interactiva

La documentación completa de la API está disponible a través de Swagger UI:

- **Desarrollo local**: `http://localhost:5046/`
- **Docker**: `http://localhost:8080/`

## Autenticación

Todos los endpoints (excepto `/api/auth/register` y `/api/auth/login`) requieren autenticación JWT.

### Obtener Token

```bash
POST /api/auth/login
Content-Type: application/json

{
  "username": "tu_usuario",
  "password": "tu_contraseña"
}
```

### Usar Token

Incluye el token en el header `Authorization`:

```
Authorization: Bearer {tu_token_jwt}
```

## Endpoints Principales

Consulta la documentación de Swagger para ver todos los endpoints disponibles, sus parámetros, y ejemplos de uso.

### Categorías de Endpoints

- **Auth** (`/api/auth`) - Autenticación y registro
- **Users** (`/api/users`) - Gestión de usuarios
- **Roles** (`/api/roles`) - Gestión de roles
- **Projects** (`/api/projects`) - Gestión de proyectos
- **Sprints** (`/api/sprints`) - Gestión de sprints
- **Incidents** (`/api/incidents`) - Gestión de incidencias
- **Dashboard** (`/api/dashboard`) - Métricas y estadísticas
- **Audit** (`/api/audit`) - Logs de auditoría
- **Notifications** (`/api/notifications`) - Notificaciones
- **Admin** (`/api/admin`) - Funciones administrativas

## Configuración

Ver [README.md](../README.md) en la raíz del proyecto para instrucciones completas de configuración y despliegue.

## Soporte

Para más información sobre requerimientos funcionales y no funcionales, consulta:
- [Requerimientos.md](../Requerimientos.md)
- [README.md](../README.md) principal
