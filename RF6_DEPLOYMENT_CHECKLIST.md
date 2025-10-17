# RF6 - Lista de Verificación para Despliegue

## Pre-Despliegue

### 1. Configuración de Resend (RF6.3 - Notificaciones por Email)

- [ ] Crear cuenta en https://resend.com
- [ ] Verificar dominio de envío
- [ ] Generar API key de producción
- [ ] Actualizar `appsettings.Production.json`:
  ```json
  "EmailSettings": {
    "ResendApiKey": "re_XXXXXXXXXXXX",
    "FromEmail": "noreply@tudominio.com",
    "FromName": "BugMgr System"
  }
  ```
- [ ] Probar envío de email de prueba

### 2. Configuración de PostgreSQL (RF6.1, RF6.2 - Backup/Restore)

- [ ] Verificar instalación de PostgreSQL tools:
  ```bash
  which pg_dump
  which pg_restore
  ```
- [ ] Verificar permisos de usuario de base de datos
- [ ] Actualizar ruta en `appsettings.Production.json`:
  ```json
  "BackupSettings": {
    "StoragePath": "/var/backups/bugmgr",
    "PostgresPath": "/usr/bin"
  }
  ```
- [ ] Crear directorio de backups:
  ```bash
  sudo mkdir -p /var/backups/bugmgr
  sudo chown www-data:www-data /var/backups/bugmgr
  sudo chmod 750 /var/backups/bugmgr
  ```

### 3. Configuración de Almacenamiento (RF6.4, RF6.5 - Adjuntos)

- [ ] Crear directorio de uploads:
  ```bash
  sudo mkdir -p /var/www/bugmgr/uploads
  sudo chown www-data:www-data /var/www/bugmgr/uploads
  sudo chmod 750 /var/www/bugmgr/uploads
  ```
- [ ] Actualizar configuración en `appsettings.Production.json`:
  ```json
  "FileSettings": {
    "StoragePath": "/var/www/bugmgr/uploads",
    "MaxFileSizeBytes": 10485760,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".txt", ".log", ".zip"]
  }
  ```
- [ ] Verificar espacio en disco disponible

### 4. Seguridad

- [ ] Actualizar JWT SecretKey en producción
- [ ] Configurar HTTPS
- [ ] Configurar CORS apropiadamente
- [ ] Revisar y aplicar políticas de firewall
- [ ] Configurar rate limiting si es necesario

### 5. Migraciones de Base de Datos

- [ ] Verificar que las tablas existan:
  - `Backups`
  - `Restores`
  - `Attachments`
  - `Notifications`
- [ ] Ejecutar migraciones pendientes:
  ```bash
  dotnet ef database update
  ```

## Post-Despliegue

### 1. Pruebas de Funcionalidad

#### RF6.1 - Backup
- [ ] Crear backup manual desde API
- [ ] Verificar que el archivo se crea en el directorio
- [ ] Verificar registro en tabla `Backups`
- [ ] Verificar entrada en `AuditLogs`

#### RF6.2 - Restore
- [ ] Restaurar backup de prueba
- [ ] Verificar que no hay errores
- [ ] Verificar registro en tabla `Restores`
- [ ] Verificar entrada en `AuditLogs`

#### RF6.3 - Email Notifications
- [ ] Asignar una incidencia a un usuario
- [ ] Verificar que el email se envía
- [ ] Verificar que la notificación in-app se crea
- [ ] Cambiar estado de una incidencia
- [ ] Verificar que el email se envía
- [ ] Verificar formato del email

#### RF6.4 - Attachments
- [ ] Subir archivo válido
- [ ] Descargar archivo
- [ ] Verificar checksum
- [ ] Listar adjuntos
- [ ] Eliminar adjunto
- [ ] Verificar registros en `AuditLogs`

#### RF6.5 - File Validation
- [ ] Intentar subir archivo > 10MB (debe fallar)
- [ ] Intentar subir extensión no permitida (debe fallar)
- [ ] Verificar mensajes de error apropiados

### 2. Monitoreo

- [ ] Configurar alertas para fallos de backup
- [ ] Configurar alertas para fallos de envío de email
- [ ] Configurar alertas de espacio en disco
- [ ] Revisar logs de aplicación
- [ ] Verificar que las métricas se están recopilando

### 3. Backup Automatizado

- [ ] Configurar cron job para backups automáticos:
  ```bash
  # Ejemplo: Backup diario a las 2 AM
  0 2 * * * curl -X POST https://api.bugmgr.com/api/backup \
    -H "Authorization: Bearer $BACKUP_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"notes":"Automatic daily backup"}'
  ```
- [ ] Configurar rotación de backups antiguos
- [ ] Configurar copia de backups a ubicación externa

### 4. Documentación

- [ ] Actualizar documentación de operaciones
- [ ] Documentar ubicaciones de directorios
- [ ] Documentar procedimientos de emergencia
- [ ] Actualizar runbook de soporte

## Rollback Plan

En caso de problemas durante el despliegue:

### 1. Reverter Código
```bash
git revert d7b3dcd
dotnet build
dotnet publish
```

### 2. Reverter Configuración
- Restaurar `appsettings.Production.json` anterior
- Reiniciar aplicación

### 3. Verificar Estado
- Verificar que la aplicación inicia
- Verificar que las funcionalidades básicas funcionan
- Revisar logs de errores

## Checklist de Validación Final

- [ ] ✅ Build sin errores
- [ ] ✅ Todas las migraciones aplicadas
- [ ] ✅ Directorios creados con permisos correctos
- [ ] ✅ Configuración de Resend validada
- [ ] ✅ PostgreSQL tools disponibles
- [ ] ✅ Backups manuales funcionando
- [ ] ✅ Restore probado exitosamente
- [ ] ✅ Emails enviándose correctamente
- [ ] ✅ Adjuntos subiéndose y descargándose
- [ ] ✅ Validaciones de archivo funcionando
- [ ] ✅ Auditoría registrándose correctamente
- [ ] ✅ Monitoreo configurado
- [ ] ✅ Documentación actualizada

## Contactos de Soporte

### Resend
- Dashboard: https://resend.com/dashboard
- Docs: https://resend.com/docs

### PostgreSQL
- Docs: https://www.postgresql.org/docs/

### Issues
- GitHub: https://github.com/Vermize13/Portal_Back_Nuevo/issues

## Notas Adicionales

### Consideraciones de Rendimiento
- Los backups pueden tardar varios minutos en bases de datos grandes
- El envío de emails es asíncrono pero puede fallar si Resend está down
- Los adjuntos grandes pueden tardar en subirse/descargarse

### Límites Conocidos
- Tamaño máximo de archivo: 10 MB (configurable)
- Resend free tier: 100 emails/día
- Backups ocupan espacio en disco, planificar rotación

### Mejoras Futuras
- Implementar backups en cloud (S3/Azure)
- Agregar compresión de backups
- Implementar caché para adjuntos frecuentes
- Agregar escaneo de virus para adjuntos

---

**Última actualización**: 17 de Octubre, 2025
**Versión**: 1.0.0
