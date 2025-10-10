**Módulos a desarrollar**

**1. Frontend (Angular)**

|**Módulo**|**Funcionalidad principal**|
| :- | :- |
|**Autenticación y Perfil**|Login/Logout, recuperación de contraseña, actualización de perfil, gestión de roles y permisos visibles.|
|**Gestión de Usuarios**|CRUD de usuarios, asignación de roles y proyectos, listado y búsqueda de usuarios, filtrado por rol o estado.|
|**Gestión de Proyectos**|CRUD de proyectos, asignación de miembros, visualización de sprints, tablero Kanban por proyecto.|
|**Gestión de Incidencias**|Crear/editar/incidencias, asignación a usuarios, subir adjuntos, comentarios, historial de cambios, etiquetas.|
|**Dashboards Dinámicos**|Visualización de métricas, gráficos de incidencias por estado, prioridad y MTTR, filtrado dinámico por proyecto o sprint.|
|**Auditoría / Logs**|Consultar logs de auditoría, filtrar por usuario, acción y fecha, exportar registros.|
|**Notificaciones**|Panel de notificaciones internas y alertas sobre asignaciones de incidencias o cambios de estado.|
|**Administración y Servicios**|Backup y restauración de base de datos, gestión de archivos adjuntos, configuración de parámetros del sistema.|

-----
**2. Backend (C# / .NET)**

|**Módulo**|**Funcionalidad principal**|
| :- | :- |
|**API de Autenticación**|Login/Logout, JWT, recuperación de contraseña, roles y permisos, validación de sesión.|
|**Usuarios**|Servicios CRUD de usuarios, asignación de roles, control de acceso, auditoría de cambios.|
|**Proyectos**|CRUD de proyectos, gestión de miembros, asociación de sprints, estado de avance de proyectos.|
|**Incidencias**|CRUD de incidencias, asignación, historial de cambios, manejo de adjuntos, notificaciones, etiquetas.|
|**Dashboards / Métricas**|Servicios para obtener datos agregados para gráficos, cálculo de MTTR, estadísticas de incidencias por estado, prioridad o sprint.|
|**Auditoría**|Registrar todas las acciones del sistema, consultar logs, exportar registros.|
|**Notificaciones**|Servicio de envío de notificaciones internas o por correo electrónico.|
|**Gestión de Archivos y Backup**|Subida/descarga de adjuntos, validación de tamaño y tipo, backup y restauración de base de datos.|
|**Servicios Compartidos / Core**|Configuración general, conexión a la base de datos, validaciones comunes, manejo de errores y logs.|

