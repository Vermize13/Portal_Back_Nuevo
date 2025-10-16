# Dashboard and Audit API Documentation

This document describes the API endpoints for Dynamic Dashboards (RF4) and Audit (RF5) features.

## Table of Contents
- [Dashboard Endpoints (RF4)](#dashboard-endpoints-rf4)
- [Audit Endpoints (RF5)](#audit-endpoints-rf5)

---

## Dashboard Endpoints (RF4)

All dashboard endpoints require authentication via JWT token.

### RF4.1: Get Incident Metrics

Get metrics of incidents grouped by status, priority, and severity.

**Endpoint:** `POST /api/dashboard/metrics`

**Request Body:**
```json
{
  "projectId": "guid-optional",
  "sprintId": "guid-optional",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```

**Response:**
```json
{
  "byStatus": {
    "Open": 15,
    "InProgress": 8,
    "Resolved": 25,
    "Closed": 30,
    "Rejected": 2,
    "Duplicated": 1
  },
  "byPriority": {
    "Must": 20,
    "Should": 35,
    "Could": 15,
    "Wont": 5
  },
  "bySeverity": {
    "Critical": 5,
    "High": 15,
    "Medium": 35,
    "Low": 20
  }
}
```

### RF4.2: Get Sprint Incidents

Get the number of incidents opened and closed by sprint.

**Endpoint:** `GET /api/dashboard/sprints?projectId={guid}`

**Query Parameters:**
- `projectId` (optional): Filter by specific project

**Response:**
```json
[
  {
    "sprintId": "guid",
    "sprintName": "Sprint 1",
    "openedCount": 25,
    "closedCount": 20,
    "startDate": "2024-01-01",
    "endDate": "2024-01-14"
  },
  {
    "sprintId": "guid",
    "sprintName": "Sprint 2",
    "openedCount": 30,
    "closedCount": 28,
    "startDate": "2024-01-15",
    "endDate": "2024-01-28"
  }
]
```

### RF4.3: Calculate MTTR (Mean Time To Resolution)

Calculate the average time it takes to resolve incidents.

**Endpoint:** `POST /api/dashboard/mttr`

**Request Body:**
```json
{
  "projectId": "guid-optional",
  "sprintId": "guid-optional",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```

**Response:**
```json
{
  "averageMTTRHours": 48.75,
  "averageMTTRDays": 2.03,
  "resolvedIncidentsCount": 55
}
```

### RF4.4: Get Incident Evolution

Get the evolution of incidents over time (opened, closed, and total open).

**Endpoint:** `POST /api/dashboard/evolution`

**Request Body:**
```json
{
  "projectId": "guid-optional",
  "sprintId": "guid-optional",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-01-31T23:59:59Z"
}
```

**Response:**
```json
[
  {
    "date": "2024-01-01",
    "openedCount": 5,
    "closedCount": 2,
    "totalOpenCount": 8
  },
  {
    "date": "2024-01-02",
    "openedCount": 3,
    "closedCount": 4,
    "totalOpenCount": 7
  }
]
```

---

## Audit Endpoints (RF5)

All audit endpoints require authentication via JWT token.

### RF5.1: Audit Logging (Automatic)

Audit logging is automatically performed by the system for all user actions:
- Login/Logout
- Create/Update/Delete operations
- Assign operations
- State transitions
- File uploads/downloads
- Backup/Restore operations

No manual endpoint call is required. The logging is handled internally by the `AuditService`.

### RF5.2: Filter Audit Logs

Retrieve audit logs with filtering and pagination.

**Endpoint:** `POST /api/audit/logs`

**Request Body:**
```json
{
  "userId": "guid-optional",
  "action": "Login",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "page": 1,
  "pageSize": 50
}
```

**Available Actions:**
- `Create`
- `Update`
- `Delete`
- `Login`
- `Logout`
- `Assign`
- `Transition`
- `Backup`
- `Restore`
- `Upload`
- `Download`

**Response:**
```json
{
  "logs": [
    {
      "id": "guid",
      "action": "Login",
      "actorId": "guid",
      "actorUsername": "john.doe",
      "entityName": null,
      "entityId": null,
      "ipAddress": "192.168.1.100",
      "userAgent": "Mozilla/5.0...",
      "detailsJson": "{\"username\":\"john.doe\"}",
      "createdAt": "2024-01-15T10:30:00Z"
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 50,
  "totalPages": 3
}
```

### RF5.3: Export Audit Logs

Export audit logs to CSV format.

**Endpoint:** `POST /api/audit/export`

**Request Body:**
```json
{
  "userId": "guid-optional",
  "action": "Login",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```

**Response:**
- Content-Type: `text/csv`
- File: `audit_logs_20241231_143500.csv`

**CSV Format:**
```csv
Id,Action,ActorId,ActorUsername,EntityName,EntityId,IpAddress,UserAgent,CreatedAt,Details
guid,Login,guid,john.doe,,,192.168.1.100,Mozilla/5.0...,2024-01-15 10:30:00,"{""username"":""john.doe""}"
```

---

## Authentication

All endpoints require a valid JWT token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

To obtain a token, use the authentication endpoints:
- `POST /api/auth/login`
- `POST /api/auth/register`

---

## Error Responses

### 401 Unauthorized
```json
{
  "message": "Unauthorized access"
}
```

### 400 Bad Request
```json
{
  "message": "Invalid request parameters"
}
```

### 500 Internal Server Error
```json
{
  "message": "An error occurred while processing your request"
}
```

---

## Notes

1. **Date Filters**: All date filters support ISO 8601 format timestamps.
2. **Pagination**: Default page size is 50 for audit logs. Maximum page size is configurable.
3. **Performance**: Dashboard queries are optimized for large datasets, typically responding in under 3 seconds (RNF2.2).
4. **CSV Export**: Large exports may take longer. Consider using date filters to limit the result set.
5. **Time Zones**: All timestamps are stored and returned in UTC.
