# Audit Logging Implementation

This document describes the comprehensive audit logging system implemented in the Portal_Back_Nuevo backend.

## Overview

The audit logging system captures two types of audits:
1. **HTTP Request Auditing** - Tracks all HTTP requests to the API
2. **SQL Command Auditing** - Logs all SQL statements executed by Entity Framework Core

## Components

### 1. AuditLog Entity

Located in `Domain/Entity/AuditLog.cs`, this entity stores all audit records with the following fields:

**Common Fields:**
- `Id` - Unique identifier for the audit log
- `Action` - Type of audit action (HttpRequest, SqlCommand, Create, Update, etc.)
- `ActorId` - User ID who performed the action (if authenticated)
- `RequestId` - Unique identifier for correlating related audit entries
- `IpAddress` - IP address of the client
- `UserAgent` - User agent string from the client
- `DetailsJson` - Additional details in JSON format
- `CreatedAt` - Timestamp when the audit was created

**HTTP Request Specific Fields:**
- `HttpMethod` - HTTP method (GET, POST, PUT, DELETE, etc.)
- `HttpPath` - Request path (e.g., /api/users)
- `HttpStatusCode` - Response status code (200, 404, 500, etc.)
- `DurationMs` - Request processing duration in milliseconds

**SQL Command Specific Fields:**
- `SqlCommand` - The SQL command text
- `SqlParameters` - JSON representation of SQL parameters

### 2. HTTP Audit Middleware

Located in `API/Middleware/HttpAuditMiddleware.cs`

**Features:**
- Captures HTTP method, path, status code, and duration for each request
- Automatically extracts authenticated user ID from JWT claims
- Records client IP address and user agent
- Uses async fire-and-forget pattern to avoid blocking HTTP responses
- Excludes specific paths from auditing:
  - `/swagger` - Swagger UI and documentation
  - `/health` - Health check endpoints
  - `/favicon.ico` - Browser favicon requests

**Usage:**
The middleware is automatically registered in `Program.cs`:
```csharp
app.UseHttpAuditing();
```

### 3. SQL Command Interceptor

Located in `Infrastructure/SqlCommandAuditInterceptor.cs`

**Features:**
- Intercepts all SQL commands executed by Entity Framework Core
- Logs command text and parameters to the standard logger
- Prevents infinite recursion by skipping audit log inserts
- Can be extended to persist SQL audits to the database

**How it works:**
- The interceptor is registered in the Infrastructure layer's service collection
- It intercepts `ReaderExecuting`, `NonQueryExecuting`, and `ScalarExecuting` events
- SQL commands affecting the AuditLogs table are automatically excluded to prevent recursion

### 4. Audit Service

Located in `API/Services/AuditService.cs` and `IAuditService.cs`

**Methods:**
- `LogAsync()` - Original method for logging business actions
- `LogHttpRequestAsync()` - Logs HTTP request audits
- `LogSqlCommandAsync()` - Logs SQL command audits (for future use)

## Database Schema

A migration has been created to add the new fields to the AuditLogs table:
- File: `Infrastructure/Migrations/20251122120000_AddAuditLogHttpAndSqlFields.cs`

To apply the migration:
```bash
dotnet ef database update --startup-project API/API.csproj
```

## Configuration

### Excluding Paths from HTTP Auditing

To exclude additional paths from HTTP auditing, modify the `HttpAuditMiddleware` constructor:

```csharp
_excludedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "/swagger",
    "/health",
    "/favicon.ico",
    "/custom-path"  // Add your path here
};
```

### Customizing SQL Audit Behavior

The SQL interceptor currently logs to the standard logger. To persist SQL audits to the database:

1. Uncomment or add database persistence code in `SqlCommandAuditInterceptor.LogCommand()`
2. Use a background service to avoid blocking SQL queries
3. Consider using a separate database connection to avoid recursion issues

## Querying Audit Logs

### Get all HTTP requests for a user:
```sql
SELECT * FROM "AuditLogs" 
WHERE "Action" = 'HttpRequest' 
AND "ActorId" = 'user-guid-here'
ORDER BY "CreatedAt" DESC;
```

### Get failed HTTP requests:
```sql
SELECT * FROM "AuditLogs" 
WHERE "Action" = 'HttpRequest' 
AND "HttpStatusCode" >= 400
ORDER BY "CreatedAt" DESC;
```

### Get slow requests (> 1 second):
```sql
SELECT * FROM "AuditLogs" 
WHERE "Action" = 'HttpRequest' 
AND "DurationMs" > 1000
ORDER BY "DurationMs" DESC;
```

### Get audit logs by endpoint:
```sql
SELECT * FROM "AuditLogs" 
WHERE "Action" = 'HttpRequest' 
AND "HttpPath" LIKE '/api/users%'
ORDER BY "CreatedAt" DESC;
```

## Testing

Unit tests have been implemented for:
- `AuditService` - Tests for all audit logging methods
- `HttpAuditMiddleware` - Tests for HTTP request auditing

Run tests with:
```bash
dotnet test API.Tests/API.Tests.csproj --filter "FullyQualifiedName~Audit"
```

## Security Considerations

1. **Sensitive Data**: Be careful not to log sensitive information like passwords, tokens, or personal data
2. **Performance**: HTTP auditing uses fire-and-forget async to minimize impact on request performance
3. **SQL Auditing**: SQL command logging can generate significant volume - consider implementing retention policies
4. **Access Control**: Implement appropriate access controls for querying audit logs

## Future Enhancements

1. **Filtering**: Add configuration-based filtering for which endpoints to audit
2. **Sampling**: Implement sampling to reduce audit volume for high-traffic endpoints
3. **Retention**: Add automated retention/archival policies for old audit logs
4. **Analytics**: Create dashboard views for audit log analysis
5. **Alerts**: Implement alerting for suspicious patterns (e.g., multiple failed logins)
6. **SQL Persistence**: Move SQL auditing to database storage with background processing
