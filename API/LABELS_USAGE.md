# Labels API Documentation

This document describes the Labels API endpoints for creating and managing project labels.

## Endpoints

### 1. Create Label

Creates a new label for a project.

**Endpoint:** `POST /api/labels`

**Authorization:** Required (JWT Bearer token)

**Request Body:**
```json
{
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Bug",
  "colorHex": "#FF0000"
}
```

**Request Parameters:**
- `projectId` (required): GUID of the project the label belongs to
- `name` (required): Name of the label (1-50 characters)
- `colorHex` (optional): Hex color code for the label (e.g., #FF0000, #F00)

**Success Response (201 Created):**
```json
{
  "id": "8fa85f64-5717-4562-b3fc-2c963f66afa7",
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Bug",
  "colorHex": "#FF0000"
}
```

**Error Responses:**
- `400 Bad Request`: Invalid request data or validation errors
- `404 Not Found`: Project not found
- `401 Unauthorized`: Missing or invalid authentication token

**Example cURL:**
```bash
curl -X POST "https://api.example.com/api/labels" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Bug",
    "colorHex": "#FF0000"
  }'
```

---

### 2. Get Project Labels

Retrieves all labels for a specific project.

**Endpoint:** `GET /api/labels/project/{projectId}`

**Authorization:** Required (JWT Bearer token)

**URL Parameters:**
- `projectId` (required): GUID of the project

**Success Response (200 OK):**
```json
[
  {
    "id": "8fa85f64-5717-4562-b3fc-2c963f66afa7",
    "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Bug",
    "colorHex": "#FF0000"
  },
  {
    "id": "9fa85f64-5717-4562-b3fc-2c963f66afa8",
    "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Feature",
    "colorHex": "#00FF00"
  }
]
```

**Error Responses:**
- `404 Not Found`: Project not found
- `401 Unauthorized`: Missing or invalid authentication token

**Example cURL:**
```bash
curl -X GET "https://api.example.com/api/labels/project/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## Usage Examples

### Creating Labels for a Project

```javascript
// JavaScript/TypeScript example
const createLabel = async (projectId, name, colorHex) => {
  const response = await fetch('https://api.example.com/api/labels', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ projectId, name, colorHex })
  });
  
  if (!response.ok) {
    throw new Error('Failed to create label');
  }
  
  return await response.json();
};

// Create multiple labels
await createLabel(projectId, 'Bug', '#FF0000');
await createLabel(projectId, 'Feature', '#00FF00');
await createLabel(projectId, 'Enhancement', '#0000FF');
```

### Retrieving Project Labels

```javascript
// JavaScript/TypeScript example
const getProjectLabels = async (projectId) => {
  const response = await fetch(
    `https://api.example.com/api/labels/project/${projectId}`,
    {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    }
  );
  
  if (!response.ok) {
    throw new Error('Failed to retrieve labels');
  }
  
  return await response.json();
};

// Get all labels for a project
const labels = await getProjectLabels(projectId);
console.log(`Found ${labels.length} labels`);
```

---

## Validation Rules

### Name
- Required
- Minimum length: 1 character
- Maximum length: 50 characters

### ColorHex
- Optional
- Must be a valid hex color code
- Formats accepted: `#RGB` or `#RRGGBB`
- Examples: `#F00`, `#FF0000`, `#00FF00`

### ProjectId
- Required
- Must be a valid GUID
- Project must exist in the database

---

## Audit Logging

All label creation operations are automatically logged in the audit system with the following information:
- Action: Create
- Actor: User ID who created the label
- Entity: Label
- Entity ID: ID of the created label
- Details: Label name and project name
- IP Address and User Agent

---

## Related Entities

Labels are associated with:
- **Projects**: Each label belongs to one project
- **Incidents**: Labels can be assigned to incidents via the IncidentLabels join table

For information on assigning labels to incidents, see the Incidents API documentation.
