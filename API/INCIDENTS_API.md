# Incidents API Documentation

This document describes the Incident Management API endpoints implemented for the Portal_Back_Nuevo project.

## Overview

The Incidents API provides full CRUD operations for managing incidents, including:
- Creating, reading, updating incidents
- Assigning incidents to users
- Closing incidents
- Managing comments
- Managing labels/tags
- Tracking change history
- Automatic notifications on assignments and status changes
- Full audit logging

## Requirements Implemented

### RF3.1 - CRUD Operations
✅ Create, edit, assign, and close incidents

### RF3.2 - Incident Structure
✅ Incidents contain: título, descripción, severidad, prioridad, estado, sprint asociado, usuario reportante, usuario asignado

### RF3.3 - File Attachments
⚠️ Attachment upload endpoints to be implemented (entity and relationships already exist)

### RF3.4 - Change History
✅ Full change history tracking with `IncidentHistory` entity

### RF3.5 - Comments
✅ Comment management on incidents

### RF3.6 - Notifications
✅ Automatic notifications when incidents are assigned or status changes

### RF3.7 - Labels/Tags
✅ Label management for incident classification

## Endpoints

### Get All Incidents
```
GET /api/Incidents
```

Query Parameters:
- `projectId` (optional): Filter by project
- `sprintId` (optional): Filter by sprint
- `status` (optional): Filter by status (Open, InProgress, Resolved, Closed, Rejected, Duplicated)
- `severity` (optional): Filter by severity (Low, Medium, High, Critical)
- `priority` (optional): Filter by priority (Wont, Could, Should, Must)
- `assigneeId` (optional): Filter by assignee
- `reporterId` (optional): Filter by reporter

Response: Array of `IncidentResponse`

### Get Incident by ID
```
GET /api/Incidents/{id}
```

Response: `IncidentResponse`

### Create Incident
```
POST /api/Incidents
```

Body: `CreateIncidentRequest`
```json
{
  "projectId": "guid",
  "sprintId": "guid",
  "title": "string",
  "description": "string",
  "severity": "Medium",
  "priority": "Should",
  "assigneeId": "guid",
  "storyPoints": 3,
  "dueDate": "2025-10-20",
  "labelIds": ["guid1", "guid2"]
}
```

Response: `IncidentResponse` (201 Created)

Features:
- Automatic code generation (e.g., PROJ-1, PROJ-2)
- Validates project and sprint existence
- Validates assignee exists
- Adds labels if provided
- Sends notification to assignee if assigned
- Logs to audit

### Update Incident
```
PUT /api/Incidents/{id}
```

Body: `UpdateIncidentRequest`
```json
{
  "title": "string",
  "description": "string",
  "severity": "High",
  "priority": "Must",
  "status": "InProgress",
  "sprintId": "guid",
  "assigneeId": "guid",
  "storyPoints": 5,
  "dueDate": "2025-10-25"
}
```

Response: `IncidentResponse`

Features:
- Tracks all field changes in `IncidentHistory`
- Sends notification on assignment change
- Sends notification on status change
- Automatically sets `ClosedAt` when status is Closed
- Logs to audit with change details

### Assign Incident
```
POST /api/Incidents/{id}/assign/{assigneeId}
```

Response: `IncidentResponse`

Features:
- Validates assignee exists
- Tracks change in history
- Sends notification to new assignee
- Logs assignment to audit

### Close Incident
```
POST /api/Incidents/{id}/close
```

Response: `IncidentResponse`

Features:
- Sets status to Closed
- Sets ClosedAt timestamp
- Tracks change in history
- Sends notification
- Logs to audit

### Get Incident History
```
GET /api/Incidents/{id}/history
```

Response: Array of `IncidentHistory`
```json
[
  {
    "id": "guid",
    "incidentId": "guid",
    "changedBy": "guid",
    "changedByUser": {
      "name": "John Doe"
    },
    "fieldName": "Status",
    "oldValue": "Open",
    "newValue": "InProgress",
    "changedAt": "2025-10-16T10:30:00Z"
  }
]
```

### Add Comment
```
POST /api/Incidents/{id}/comments
```

Body: `AddCommentRequest`
```json
{
  "body": "This is a comment"
}
```

Response: `IncidentComment` (201 Created)

Features:
- Automatically sets author from JWT token
- Logs to audit

### Get Comments
```
GET /api/Incidents/{id}/comments
```

Response: Array of `IncidentComment`

### Add Label
```
POST /api/Incidents/{id}/labels/{labelId}
```

Response: 204 No Content

Features:
- Validates label exists
- Validates label belongs to incident's project
- Logs to audit

### Remove Label
```
DELETE /api/Incidents/{id}/labels/{labelId}
```

Response: 204 No Content

Features:
- Logs to audit

## Authorization

All endpoints require authentication via JWT Bearer token.

Example:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

## Response Models

### IncidentResponse
```json
{
  "id": "guid",
  "projectId": "guid",
  "sprintId": "guid",
  "code": "PROJ-1",
  "title": "Bug in login page",
  "description": "Users cannot log in",
  "severity": "High",
  "priority": "Must",
  "status": "Open",
  "reporterId": "guid",
  "reporterName": "John Doe",
  "assigneeId": "guid",
  "assigneeName": "Jane Smith",
  "storyPoints": 3,
  "dueDate": "2025-10-20",
  "createdAt": "2025-10-16T10:00:00Z",
  "updatedAt": "2025-10-16T10:30:00Z",
  "closedAt": null,
  "labels": [
    {
      "id": "guid",
      "name": "bug",
      "colorHex": "#ff0000"
    }
  ],
  "commentCount": 5,
  "attachmentCount": 0
}
```

## Enums

### IncidentStatus
- `Open`
- `InProgress`
- `Resolved`
- `Closed`
- `Rejected`
- `Duplicated`

### IncidentSeverity
- `Low`
- `Medium`
- `High`
- `Critical`

### IncidentPriority
- `Wont` (Won't have)
- `Could`
- `Should`
- `Must`

## Database Migration

A new migration has been created to add the `IncidentHistory` table:

```bash
dotnet ef migrations add AddIncidentHistory --startup-project ../API/API.csproj
```

To apply the migration:
```bash
dotnet ef database update --startup-project ../API/API.csproj
```

## Services

### NotificationService
- `NotifyIncidentAssignmentAsync`: Sends notification when incident is assigned
- `NotifyIncidentStatusChangeAsync`: Sends notification when status changes

### AuditService
All incident operations are automatically logged to the audit log with:
- Action type (Create, Update, Assign, Transition)
- Actor ID
- Entity details
- IP address and User-Agent
- Timestamp

## Testing

To test the API:

1. Start the API:
```bash
cd API
dotnet run
```

2. Open Swagger UI at `http://localhost:5046`

3. Authenticate:
   - Use `/api/Auth/login` to get a JWT token
   - Click "Authorize" in Swagger and enter the token

4. Test endpoints:
   - Create a project first (if not exists)
   - Create an incident with `/api/Incidents` POST
   - Get incidents with `/api/Incidents` GET
   - Update, assign, add comments, labels, etc.

## Next Steps

To complete the implementation:
- [ ] Implement file upload endpoints for attachments (RF3.3)
- [ ] Add unit tests for IncidentsController
- [ ] Add integration tests for incident workflows
- [ ] Consider implementing email notifications (currently only in-app)
- [ ] Add pagination for large incident lists
- [ ] Add bulk operations (assign multiple, close multiple)
