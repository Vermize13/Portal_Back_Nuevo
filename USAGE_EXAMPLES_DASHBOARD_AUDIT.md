# Usage Examples for Dashboard and Audit APIs

This document provides practical examples of using the Dashboard (RF4) and Audit (RF5) APIs.

## Prerequisites

First, obtain an authentication token:

```bash
# Login
curl -X POST https://api.example.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "securePassword123"
  }'

# Response
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "admin",
  "email": "admin@example.com",
  "roles": ["Admin"],
  "expiresAt": "2024-01-15T15:30:00Z"
}
```

Use this token in all subsequent requests:
```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## Dashboard Examples (RF4)

### Example 1: Get Overall Incident Metrics

Get metrics for all incidents across all projects:

```bash
curl -X POST https://api.example.com/api/dashboard/metrics \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}'
```

**Response:**
```json
{
  "byStatus": {
    "Open": 25,
    "InProgress": 15,
    "Resolved": 40,
    "Closed": 80,
    "Rejected": 3,
    "Duplicated": 2
  },
  "byPriority": {
    "Must": 45,
    "Should": 70,
    "Could": 35,
    "Wont": 15
  },
  "bySeverity": {
    "Critical": 10,
    "High": 30,
    "Medium": 75,
    "Low": 50
  }
}
```

### Example 2: Get Metrics for a Specific Project

```bash
PROJECT_ID="123e4567-e89b-12d3-a456-426614174000"

curl -X POST https://api.example.com/api/dashboard/metrics \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"projectId\": \"$PROJECT_ID\"
  }"
```

### Example 3: Get Metrics for a Date Range

Get metrics for the last month:

```bash
curl -X POST https://api.example.com/api/dashboard/metrics \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z"
  }'
```

### Example 4: Get Sprint Incidents

Get incidents by sprint for all projects:

```bash
curl -X GET https://api.example.com/api/dashboard/sprints \
  -H "Authorization: Bearer $TOKEN"
```

**Response:**
```json
[
  {
    "sprintId": "sprint-guid-1",
    "sprintName": "Sprint 1 - Foundation",
    "openedCount": 28,
    "closedCount": 25,
    "startDate": "2024-01-01",
    "endDate": "2024-01-14"
  },
  {
    "sprintId": "sprint-guid-2",
    "sprintName": "Sprint 2 - Features",
    "openedCount": 32,
    "closedCount": 30,
    "startDate": "2024-01-15",
    "endDate": "2024-01-28"
  }
]
```

### Example 5: Get Sprint Incidents for a Specific Project

```bash
PROJECT_ID="123e4567-e89b-12d3-a456-426614174000"

curl -X GET "https://api.example.com/api/dashboard/sprints?projectId=$PROJECT_ID" \
  -H "Authorization: Bearer $TOKEN"
```

### Example 6: Calculate MTTR for All Projects

```bash
curl -X POST https://api.example.com/api/dashboard/mttr \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}'
```

**Response:**
```json
{
  "averageMTTRHours": 52.35,
  "averageMTTRDays": 2.18,
  "resolvedIncidentsCount": 120
}
```

### Example 7: Calculate MTTR for a Specific Sprint

```bash
SPRINT_ID="sprint-guid-1"

curl -X POST https://api.example.com/api/dashboard/mttr \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"sprintId\": \"$SPRINT_ID\"
  }"
```

### Example 8: Get Incident Evolution (Last 30 Days)

```bash
curl -X POST https://api.example.com/api/dashboard/evolution \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"startDate\": \"$(date -u -d '30 days ago' +%Y-%m-%dT%H:%M:%SZ)\",
    \"endDate\": \"$(date -u +%Y-%m-%dT%H:%M:%SZ)\"
  }"
```

### Example 9: Comprehensive Dashboard View

Get all dashboard metrics for a specific project and date range:

```bash
PROJECT_ID="123e4567-e89b-12d3-a456-426614174000"

# Get metrics
curl -X POST https://api.example.com/api/dashboard/metrics \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"projectId\": \"$PROJECT_ID\",
    \"startDate\": \"2024-01-01T00:00:00Z\",
    \"endDate\": \"2024-01-31T23:59:59Z\"
  }" > metrics.json

# Get MTTR
curl -X POST https://api.example.com/api/dashboard/mttr \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"projectId\": \"$PROJECT_ID\",
    \"startDate\": \"2024-01-01T00:00:00Z\",
    \"endDate\": \"2024-01-31T23:59:59Z\"
  }" > mttr.json

# Get evolution
curl -X POST https://api.example.com/api/dashboard/evolution \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"projectId\": \"$PROJECT_ID\",
    \"startDate\": \"2024-01-01T00:00:00Z\",
    \"endDate\": \"2024-01-31T23:59:59Z\"
  }" > evolution.json

# Get sprint info
curl -X GET "https://api.example.com/api/dashboard/sprints?projectId=$PROJECT_ID" \
  -H "Authorization: Bearer $TOKEN" > sprints.json

echo "Dashboard data saved to: metrics.json, mttr.json, evolution.json, sprints.json"
```

---

## Audit Examples (RF5)

### Example 10: Get Recent Audit Logs (Default Pagination)

```bash
curl -X POST https://api.example.com/api/audit/logs \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "page": 1,
    "pageSize": 50
  }'
```

**Response:**
```json
{
  "logs": [
    {
      "id": "log-guid-1",
      "action": "Login",
      "actorId": "user-guid-1",
      "actorUsername": "john.doe",
      "entityName": null,
      "entityId": null,
      "ipAddress": "192.168.1.100",
      "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)...",
      "detailsJson": "{\"username\":\"john.doe\"}",
      "createdAt": "2024-01-15T14:30:00Z"
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 50,
  "totalPages": 3
}
```

### Example 11: Filter Logs by User

```bash
USER_ID="123e4567-e89b-12d3-a456-426614174001"

curl -X POST https://api.example.com/api/audit/logs \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": \"$USER_ID\",
    \"page\": 1,
    \"pageSize\": 20
  }"
```

### Example 12: Filter Logs by Action Type

Get all login attempts:

```bash
curl -X POST https://api.example.com/api/audit/logs \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "action": "Login",
    "page": 1,
    "pageSize": 50
  }'
```

### Example 13: Filter Logs by Date Range

Get logs from the last 7 days:

```bash
curl -X POST https://api.example.com/api/audit/logs \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"startDate\": \"$(date -u -d '7 days ago' +%Y-%m-%dT%H:%M:%SZ)\",
    \"endDate\": \"$(date -u +%Y-%m-%dT%H:%M:%SZ)\",
    \"page\": 1,
    \"pageSize\": 100
  }"
```

### Example 14: Combined Filters

Get all "Create" actions by a specific user in the last month:

```bash
USER_ID="123e4567-e89b-12d3-a456-426614174001"

curl -X POST https://api.example.com/api/audit/logs \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": \"$USER_ID\",
    \"action\": \"Create\",
    \"startDate\": \"2024-01-01T00:00:00Z\",
    \"endDate\": \"2024-01-31T23:59:59Z\",
    \"page\": 1,
    \"pageSize\": 50
  }"
```

### Example 15: Export Audit Logs to CSV

Export all audit logs from a date range:

```bash
curl -X POST https://api.example.com/api/audit/export \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z"
  }' \
  --output audit_logs.csv
```

### Example 16: Export Specific User Activity

```bash
USER_ID="123e4567-e89b-12d3-a456-426614174001"

curl -X POST https://api.example.com/api/audit/export \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"userId\": \"$USER_ID\",
    \"startDate\": \"2024-01-01T00:00:00Z\",
    \"endDate\": \"2024-12-31T23:59:59Z\"
  }" \
  --output "user_${USER_ID}_activity.csv"
```

### Example 17: Monitor Failed Login Attempts

Track all login attempts in the last 24 hours:

```bash
curl -X POST https://api.example.com/api/audit/logs \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"action\": \"Login\",
    \"startDate\": \"$(date -u -d '24 hours ago' +%Y-%m-%dT%H:%M:%SZ)\",
    \"endDate\": \"$(date -u +%Y-%m-%dT%H:%M:%SZ)\",
    \"page\": 1,
    \"pageSize\": 100
  }"
```

### Example 18: Track File Operations

Get all upload and download activities:

```bash
# Get upload activities
curl -X POST https://api.example.com/api/audit/logs \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "action": "Upload",
    "page": 1,
    "pageSize": 50
  }' > uploads.json

# Get download activities
curl -X POST https://api.example.com/api/audit/logs \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "action": "Download",
    "page": 1,
    "pageSize": 50
  }' > downloads.json
```

---

## JavaScript/TypeScript Examples

### Example 19: Fetch Dashboard Data with Fetch API

```javascript
const API_BASE_URL = 'https://api.example.com';
const TOKEN = 'your-jwt-token-here';

async function getDashboardMetrics(projectId = null, startDate = null, endDate = null) {
  const response = await fetch(`${API_BASE_URL}/api/dashboard/metrics`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${TOKEN}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      projectId,
      startDate,
      endDate
    })
  });
  
  if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
  }
  
  return await response.json();
}

// Usage
getDashboardMetrics()
  .then(metrics => {
    console.log('Incident Metrics:', metrics);
    console.log('Open incidents:', metrics.byStatus.Open);
    console.log('Critical incidents:', metrics.bySeverity.Critical);
  })
  .catch(error => console.error('Error:', error));
```

### Example 20: Fetch Audit Logs with Axios

```javascript
import axios from 'axios';

const API_BASE_URL = 'https://api.example.com';
const TOKEN = 'your-jwt-token-here';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Authorization': `Bearer ${TOKEN}`,
    'Content-Type': 'application/json'
  }
});

async function getAuditLogs(filters = {}) {
  try {
    const response = await api.post('/api/audit/logs', {
      userId: filters.userId || null,
      action: filters.action || null,
      startDate: filters.startDate || null,
      endDate: filters.endDate || null,
      page: filters.page || 1,
      pageSize: filters.pageSize || 50
    });
    
    return response.data;
  } catch (error) {
    console.error('Error fetching audit logs:', error);
    throw error;
  }
}

// Usage
getAuditLogs({
  action: 'Login',
  startDate: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
  endDate: new Date().toISOString()
})
  .then(result => {
    console.log(`Total logs: ${result.totalCount}`);
    console.log(`Pages: ${result.totalPages}`);
    result.logs.forEach(log => {
      console.log(`${log.createdAt}: ${log.actorUsername} performed ${log.action}`);
    });
  });
```

---

## Python Examples

### Example 21: Fetch Dashboard Data with Requests

```python
import requests
from datetime import datetime, timedelta

API_BASE_URL = 'https://api.example.com'
TOKEN = 'your-jwt-token-here'

headers = {
    'Authorization': f'Bearer {TOKEN}',
    'Content-Type': 'application/json'
}

def get_mttr(project_id=None, sprint_id=None, start_date=None, end_date=None):
    payload = {
        'projectId': project_id,
        'sprintId': sprint_id,
        'startDate': start_date,
        'endDate': end_date
    }
    
    response = requests.post(
        f'{API_BASE_URL}/api/dashboard/mttr',
        headers=headers,
        json=payload
    )
    
    response.raise_for_status()
    return response.json()

# Usage: Get MTTR for last 30 days
end_date = datetime.utcnow()
start_date = end_date - timedelta(days=30)

mttr = get_mttr(
    start_date=start_date.isoformat() + 'Z',
    end_date=end_date.isoformat() + 'Z'
)

print(f"Average MTTR: {mttr['averageMTTRDays']:.2f} days")
print(f"Resolved incidents: {mttr['resolvedIncidentsCount']}")
```

### Example 22: Export and Process Audit Logs

```python
import requests
import csv
from io import StringIO

API_BASE_URL = 'https://api.example.com'
TOKEN = 'your-jwt-token-here'

headers = {
    'Authorization': f'Bearer {TOKEN}',
    'Content-Type': 'application/json'
}

def export_audit_logs(user_id=None, action=None, start_date=None, end_date=None):
    payload = {
        'userId': user_id,
        'action': action,
        'startDate': start_date,
        'endDate': end_date
    }
    
    response = requests.post(
        f'{API_BASE_URL}/api/audit/export',
        headers=headers,
        json=payload
    )
    
    response.raise_for_status()
    return response.content.decode('utf-8')

# Usage: Export and analyze
csv_data = export_audit_logs(action='Login')
csv_reader = csv.DictReader(StringIO(csv_data))

login_count = 0
unique_users = set()

for row in csv_reader:
    login_count += 1
    if row['ActorUsername']:
        unique_users.add(row['ActorUsername'])

print(f"Total logins: {login_count}")
print(f"Unique users: {len(unique_users)}")
```

---

## Common Integration Patterns

### Pattern 1: Real-time Dashboard Updates

```javascript
// Poll dashboard metrics every 30 seconds
setInterval(async () => {
  const metrics = await getDashboardMetrics();
  updateDashboardUI(metrics);
}, 30000);
```

### Pattern 2: Audit Trail for Compliance

```python
# Daily export of audit logs for compliance
from datetime import datetime, timedelta
import schedule

def daily_audit_export():
    yesterday = datetime.utcnow() - timedelta(days=1)
    start = yesterday.replace(hour=0, minute=0, second=0)
    end = yesterday.replace(hour=23, minute=59, second=59)
    
    csv_data = export_audit_logs(
        start_date=start.isoformat() + 'Z',
        end_date=end.isoformat() + 'Z'
    )
    
    filename = f"audit_{yesterday.strftime('%Y%m%d')}.csv"
    with open(filename, 'w') as f:
        f.write(csv_data)
    
    print(f"Audit log exported: {filename}")

schedule.every().day.at("01:00").do(daily_audit_export)
```

---

## Notes

1. Replace `https://api.example.com` with your actual API endpoint
2. Replace `$TOKEN` with your actual JWT token
3. All date/time values should be in ISO 8601 format (UTC)
4. GUIDs should be valid UUID v4 format
5. For production use, implement proper error handling and token refresh logic
