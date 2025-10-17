# Implementación RF6 - Administración y Servicios

## Resumen

Este documento describe la implementación de los requerimientos funcionales RF6, que incluyen copias de seguridad, restauración de bases de datos, notificaciones por correo electrónico y gestión de archivos adjuntos.

## Requerimientos Implementados

### RF6.1: Copias de seguridad de la base de datos ✅
- **Endpoint**: `POST /api/backup`
- **Descripción**: Crea una copia de seguridad de la base de datos PostgreSQL usando `pg_dump`
- **Características**:
  - Genera archivos de backup con timestamp
  - Almacena metadatos en la tabla `Backups`
  - Registra el tamaño del archivo
  - Audita la operación

### RF6.2: Restauración de copias de seguridad ✅
- **Endpoint**: `POST /api/backup/restore`
- **Descripción**: Restaura una copia de seguridad usando `pg_restore`
- **Características**:
  - Valida que el archivo de backup exista
  - Registra la operación en la tabla `Restores`
  - Audita la operación

### RF6.3: Notificaciones por correo electrónico ✅
- **Descripción**: Sistema de notificaciones mejorado con soporte para correo electrónico
- **Proveedor**: Resend (https://resend.com)
- **Características**:
  - Notificaciones por email cuando se asigna una incidencia
  - Notificaciones por email cuando cambia el estado de una incidencia
  - Mantiene notificaciones in-app existentes
  - Manejo de errores robusto

### RF6.4: Gestión de archivos adjuntos ✅
- **Endpoints**:
  - `POST /api/incidents/{incidentId}/attachments` - Subir archivo
  - `GET /api/incidents/{incidentId}/attachments` - Listar archivos
  - `GET /api/incidents/{incidentId}/attachments/{id}` - Obtener información de archivo
  - `GET /api/incidents/{incidentId}/attachments/{id}/download` - Descargar archivo
  - `DELETE /api/incidents/{incidentId}/attachments/{id}` - Eliminar archivo
- **Características**:
  - Almacenamiento en sistema de archivos
  - Cálculo de checksum SHA256
  - Organización por incidencia
  - Auditoría de operaciones

### RF6.5: Validación de tamaño de archivos adjuntos ✅
- **Tamaño máximo por defecto**: 10 MB (10,485,760 bytes)
- **Extensiones permitidas**: .jpg, .jpeg, .png, .gif, .pdf, .txt, .log, .zip
- **Características**:
  - Validación del tamaño antes de guardar
  - Validación de extensión de archivo
  - Configuración centralizada en `appsettings.json`

## Configuración

### appsettings.json

```json
{
  "EmailSettings": {
    "ResendApiKey": "re_your_api_key_here",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "BugMgr System"
  },
  "FileSettings": {
    "StoragePath": "wwwroot/uploads",
    "MaxFileSizeBytes": 10485760,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".txt", ".log", ".zip"]
  },
  "BackupSettings": {
    "StoragePath": "backups",
    "PostgresPath": "/usr/bin"
  }
}
```

### Configuración de Resend

1. Crear una cuenta en [Resend](https://resend.com)
2. Generar una API key
3. Configurar el dominio de envío
4. Actualizar `ResendApiKey` y `FromEmail` en `appsettings.json`

### Configuración de PostgreSQL

Para que funcionen las copias de seguridad, asegúrese de que:
1. `pg_dump` y `pg_restore` estén instalados
2. La ruta en `PostgresPath` apunte al directorio correcto
3. El usuario de la base de datos tenga permisos suficientes

## Estructura de Archivos

### Servicios
- `API/Services/INotificationService.cs` - Interfaz mejorada con soporte de email
- `API/Services/NotificationService.cs` - Implementación con Resend
- `API/Services/IBackupService.cs` - Interfaz para backups
- `API/Services/BackupService.cs` - Implementación de backup/restore
- `API/Services/IAttachmentService.cs` - Interfaz para adjuntos
- `API/Services/AttachmentService.cs` - Implementación de gestión de archivos

### Controladores
- `API/Controllers/BackupController.cs` - Endpoints de backup y restore
- `API/Controllers/AttachmentsController.cs` - Endpoints de adjuntos
- `API/Controllers/IncidentsController.cs` - Actualizado para incluir conteo de adjuntos

### DTOs
- `API/DTOs/BackupRequest.cs`
- `API/DTOs/BackupResponse.cs`
- `API/DTOs/RestoreRequest.cs`
- `API/DTOs/RestoreResponse.cs`
- `API/DTOs/AttachmentResponse.cs`
- `API/DTOs/EmailSettings.cs`
- `API/DTOs/FileSettings.cs`
- `API/DTOs/BackupSettings.cs`

### Entidades (ya existentes)
- `Domain/Entity/Backup.cs`
- `Domain/Entity/Restore.cs`
- `Domain/Entity/Attachment.cs`

### Repositorios (ya existentes)
- `Repository/Repositories/BackupRepository.cs`
- `Repository/Repositories/RestoreRepository.cs`
- `Repository/Repositories/AttachmentRepository.cs`

## Uso de la API

### Crear Backup

```http
POST /api/backup
Authorization: Bearer {token}
Content-Type: application/json

{
  "notes": "Backup manual antes de actualización"
}
```

### Restaurar Backup

```http
POST /api/backup/restore
Authorization: Bearer {token}
Content-Type: application/json

{
  "backupId": "guid-del-backup",
  "notes": "Restauración por error en migración"
}
```

### Subir Adjunto

```http
POST /api/incidents/{incidentId}/attachments
Authorization: Bearer {token}
Content-Type: multipart/form-data

file: [archivo binario]
```

### Descargar Adjunto

```http
GET /api/incidents/{incidentId}/attachments/{attachmentId}/download
Authorization: Bearer {token}
```

### Listar Adjuntos de una Incidencia

```http
GET /api/incidents/{incidentId}/attachments
Authorization: Bearer {token}
```

## Seguridad

1. **Autenticación**: Todos los endpoints requieren autenticación JWT
2. **Autorización**: Solo usuarios autorizados pueden crear backups y restaurar
3. **Validación**: 
   - Tamaño máximo de archivos
   - Extensiones permitidas
   - Checksums SHA256 para integridad
4. **Auditoría**: Todas las operaciones se registran en `AuditLogs`

## Consideraciones de Producción

### Copias de Seguridad
- Configurar backups automáticos programados
- Implementar rotación de backups antiguos
- Almacenar backups en ubicación externa/cloud
- Probar regularmente el proceso de restauración

### Email
- Configurar dominio verificado en Resend
- Monitorear límites de envío
- Implementar templates HTML más sofisticados
- Considerar colas para envíos masivos

### Archivos Adjuntos
- Considerar almacenamiento en S3/Azure Blob
- Implementar limpieza de archivos huérfanos
- Configurar backup de archivos adjuntos
- Implementar escaneo de virus para archivos

## Testing

### Endpoints a Probar

1. **Backup**
   - Crear backup con y sin notas
   - Listar backups
   - Verificar que el archivo se crea
   - Verificar registro en auditoría

2. **Restore**
   - Restaurar un backup válido
   - Intentar restaurar backup inexistente
   - Verificar registro en auditoría

3. **Attachments**
   - Subir archivo válido
   - Intentar subir archivo demasiado grande
   - Intentar subir extensión no permitida
   - Descargar archivo
   - Eliminar archivo
   - Verificar checksum

4. **Email Notifications**
   - Asignar incidencia a usuario
   - Cambiar estado de incidencia
   - Verificar recepción de emails
   - Verificar que las notificaciones in-app se crean

## Mejoras Futuras

1. **Backups**
   - Backups incrementales
   - Compresión de backups
   - Backups en cloud (S3, Azure)
   - Programación de backups automáticos

2. **Email**
   - Templates HTML personalizables
   - Soporte para múltiples idiomas
   - Notificaciones configurables por usuario
   - Resúmenes diarios/semanales

3. **Attachments**
   - Vista previa de imágenes
   - Soporte para archivos grandes (chunked upload)
   - Versionado de archivos
   - Almacenamiento en cloud

## Dependencias Agregadas

- **Resend** (v0.1.6): Cliente .NET para el servicio de email Resend

## Errores Comunes y Soluciones

### Error: "pg_dump not found"
**Solución**: Actualizar `PostgresPath` en `appsettings.json` con la ruta correcta a los binarios de PostgreSQL

### Error: "File size exceeds maximum"
**Solución**: Aumentar `MaxFileSizeBytes` en `appsettings.json` o comprimir el archivo

### Error: "Failed to send email"
**Solución**: 
- Verificar que la API key de Resend sea válida
- Verificar que el dominio esté verificado en Resend
- Revisar logs para detalles del error

### Error: "Backup file not found"
**Solución**: Verificar que el directorio de backups exista y tenga permisos de escritura

## Conclusión

La implementación de RF6 proporciona funcionalidades críticas para la administración del sistema, incluyendo protección de datos mediante backups, comunicación efectiva mediante notificaciones por email, y gestión completa de archivos adjuntos con validaciones de seguridad.
