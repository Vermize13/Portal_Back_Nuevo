# OpenAPI Specification

Este archivo documenta el proceso de generación y actualización de la especificación OpenAPI del proyecto BugMgr API.

## Archivo generado

- **`openapi.json`**: Especificación OpenAPI 3.0.4 que describe todos los endpoints, modelos de datos y esquemas de autenticación de la API.

## ¿Qué incluye el openapi.json?

El archivo `openapi.json` contiene:

- **Información de la API**: Título, descripción, versión y contacto
- **Seguridad**: Configuración de autenticación JWT Bearer
- **Endpoints**: Todos los endpoints disponibles en la API con:
  - Métodos HTTP (GET, POST, PUT, DELETE, PATCH)
  - Parámetros de entrada (path, query, body)
  - Respuestas esperadas con códigos de estado
  - Tipos de contenido (application/json, multipart/form-data)
- **Schemas**: Modelos de datos utilizados en requests y responses

### Principales endpoints incluidos:

- **Auth**: Login, registro y verificación de usuario
- **Projects**: CRUD de proyectos, gestión de miembros y progreso
- **Incidents**: CRUD de incidencias, asignación, comentarios, etiquetas e historial
- **Sprints**: CRUD de sprints por proyecto
- **Attachments**: Subida, descarga y gestión de archivos adjuntos
- **Backup**: Creación y restauración de copias de seguridad
- **Users**: Consulta de usuarios

## Cómo regenerar el openapi.json

Si realizas cambios en los controladores, DTOs o modelos de la API, necesitas regenerar el archivo `openapi.json`:

### Prerrequisitos

1. Tener instalado .NET 9.0 SDK
2. Tener instalada la herramienta Swashbuckle.AspNetCore.Cli:

```bash
dotnet tool install -g Swashbuckle.AspNetCore.Cli
```

### Pasos para regenerar

1. **Compilar el proyecto**:

```bash
cd /ruta/al/proyecto
dotnet build API/API.csproj
```

2. **Generar el archivo OpenAPI**:

```bash
cd API
swagger tofile --output ../openapi.json bin/Debug/net9.0/API.dll v1
```

El archivo `openapi.json` se creará/actualizará en la raíz del proyecto.

### Validar el archivo generado

Para validar que el JSON es correcto:

```bash
cat openapi.json | python3 -m json.tool > /dev/null && echo "✓ JSON válido" || echo "✗ JSON inválido"
```

## Uso del openapi.json en el frontend

Este archivo puede ser utilizado en el frontend para:

1. **Generación automática de código**: Usar herramientas como:
   - [OpenAPI Generator](https://openapi-generator.tech/)
   - [Swagger Codegen](https://swagger.io/tools/swagger-codegen/)
   - [orval](https://orval.dev/) (para TypeScript/React)

2. **Documentación interactiva**: Importar en:
   - [Swagger UI](https://swagger.io/tools/swagger-ui/)
   - [Postman](https://www.postman.com/)
   - [Insomnia](https://insomnia.rest/)

3. **Validación de requests/responses**: Usar librerías de validación que soporten OpenAPI

### Ejemplo: Generar cliente TypeScript

```bash
npx @openapitools/openapi-generator-cli generate \
  -i openapi.json \
  -g typescript-axios \
  -o ./src/api-client
```

## Versión de OpenAPI

- **Versión de especificación**: OpenAPI 3.0.4
- **Versión de la API**: v1

## Notas adicionales

- El archivo se genera automáticamente desde el código fuente usando Swagger/Swashbuckle
- Los comentarios XML en los controladores (summary, remarks) se incluyen en las descripciones
- La configuración de JWT está incluida en el esquema de seguridad
- Los enums (IncidentStatus, IncidentPriority, IncidentSeverity, etc.) están definidos con sus valores posibles
