**1. Requerimientos Funcionales (RF)**

**RF1 – Gestión de Usuarios**

- RF1.1: El sistema debe permitir crear, editar, eliminar y desactivar usuarios.
- RF1.2: El sistema debe manejar roles globales (Admin, Product Owner, Developer, Tester).
- RF1.3: El sistema debe permitir asignar usuarios a proyectos con un rol específico.
- RF1.4: El usuario debe poder actualizar su perfil y contraseña.
- RF1.5: El sistema debe registrar en auditoría las acciones sobre usuarios.

**RF2 – Gestión de Proyectos**

- RF2.1: El sistema debe permitir crear, editar y eliminar proyectos.
- RF2.2: Cada proyecto debe tener miembros asignados con distintos roles.
- RF2.3: El sistema debe permitir asociar sprints a proyectos con fechas de inicio y fin.
- RF2.4: El sistema debe mostrar el estado de avance de cada proyecto.

**RF3 – Gestión de Incidencias**

- RF3.1: El sistema debe permitir crear, editar, asignar y cerrar incidencias.
- RF3.2: Una incidencia debe contener: título, descripción, severidad, prioridad, estado, sprint asociado, usuario reportante, usuario asignado y adjuntos.
- RF3.3: El sistema debe permitir adjuntar archivos (imágenes, logs, documentos) a las incidencias.
- RF3.4: El sistema debe mantener un historial de cambios en cada incidencia (estado, responsable, prioridad, etc.).
- RF3.5: El sistema debe permitir añadir comentarios en las incidencias.
- RF3.6: El sistema debe notificar al usuario asignado cuando reciba una incidencia.
- RF3.7: El sistema debe soportar etiquetas (tags) para clasificar incidencias.

**RF4 – Dashboards Dinámicos**

- RF4.1: El sistema debe mostrar métricas de incidencias por estado, prioridad y severidad.
- RF4.2: El sistema debe mostrar el número de incidencias abiertas y cerradas por sprint.
- RF4.3: El sistema debe calcular el tiempo medio de resolución de incidencias (MTTR).
- RF4.4: El sistema debe mostrar gráficos dinámicos de evolución de incidencias.

**RF5 – Auditoría**

- RF5.1: El sistema debe registrar todas las acciones de los usuarios (login, creación, modificación, eliminación, asignación, cambio de estado, subida/descarga de archivos).
- RF5.2: El sistema debe permitir filtrar logs de auditoría por usuario, acción y fecha.
- RF5.3: El sistema debe permitir exportar registros de auditoría.

**RF6 – Administración y Servicios**

- RF6.1: El sistema debe permitir realizar copias de seguridad de la base de datos.
- RF6.2: El sistema debe permitir restaurar una copia de seguridad.
- RF6.3: El sistema debe notificar a los usuarios sobre asignaciones y cambios en incidencias.
- RF6.4: El sistema debe gestionar archivos adjuntos asociados a incidencias.
- RF6.5: El sistema debe garantizar que los adjuntos no superen un tamaño máximo definido.
-----
**2. Requerimientos No Funcionales (RNF)**

**RNF1 – Seguridad**

- RNF1.1: El sistema debe usar autenticación con JWT.
- RNF1.2: Las contraseñas deben almacenarse con hash seguro (bcrypt/argon2).
- RNF1.3: Solo usuarios autorizados podrán acceder a funciones según su rol.

**RNF2 – Disponibilidad y Rendimiento**

- RNF2.1: El sistema debe soportar al menos 200 usuarios concurrentes.
- RNF2.2: Las consultas de dashboard deben responder en menos de 3 segundos.

**RNF3 – Usabilidad**

- RNF3.1: El sistema debe tener interfaz web responsiva (Angular + Material Design).
- RNF3.2: El tablero Kanban debe permitir mover incidencias entre estados de forma visual.

**RNF4 – Mantenibilidad**

- RNF4.1: El backend debe implementarse con C# (.NET 6 o superior) y buenas prácticas de arquitectura (capas, inyección de dependencias, repositorios).
- RNF4.2: El frontend debe estar implementado en Angular.
- RNF4.3: La base de datos debe implementarse en PostgreSQL con control de migraciones (EF Core).

**RNF5 – Portabilidad**

- RNF5.1: El sistema debe poder desplegarse en contenedores Docker.
- RNF5.2: El sistema debe ser accesible en navegadores modernos (Chrome, Firefox, Edge).

