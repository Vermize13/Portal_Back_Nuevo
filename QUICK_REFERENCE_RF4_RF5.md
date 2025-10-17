# Quick Reference: RF4 & RF5 Endpoints

## Dashboard Endpoints (RF4) 📊

All endpoints require: `Authorization: Bearer <token>`

### 1. Get Incident Metrics (RF4.1)
```bash
POST /api/dashboard/metrics
{
  "projectId": "optional-guid",
  "sprintId": "optional-guid",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```
**Returns:** Incidents grouped by status, priority, and severity

### 2. Get Sprint Incidents (RF4.2)
```bash
GET /api/dashboard/sprints?projectId=optional-guid
```
**Returns:** List of sprints with opened/closed incident counts

### 3. Calculate MTTR (RF4.3)
```bash
POST /api/dashboard/mttr
{
  "projectId": "optional-guid",
  "sprintId": "optional-guid",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```
**Returns:** Average resolution time in hours and days

### 4. Get Incident Evolution (RF4.4)
```bash
POST /api/dashboard/evolution
{
  "projectId": "optional-guid",
  "sprintId": "optional-guid",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```
**Returns:** Daily breakdown of opened/closed incidents

---

## Audit Endpoints (RF5) 🔍

All endpoints require: `Authorization: Bearer <token>`

### 1. Filter Audit Logs (RF5.2)
```bash
POST /api/audit/logs
{
  "userId": "optional-guid",
  "action": "Login",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "page": 1,
  "pageSize": 50
}
```
**Returns:** Paginated audit logs with filtering

**Available Actions:**
- `Create`, `Update`, `Delete`
- `Login`, `Logout`
- `Assign`, `Transition`
- `Backup`, `Restore`
- `Upload`, `Download`

### 2. Export Audit Logs (RF5.3)
```bash
POST /api/audit/export
{
  "userId": "optional-guid",
  "action": "Login",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z"
}
```
**Returns:** CSV file with audit logs

---

## Quick Examples

### Get Dashboard Overview
```bash
TOKEN="your-jwt-token"

# Get all metrics
curl -X POST http://localhost:5000/api/dashboard/metrics \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}'

# Get MTTR
curl -X POST http://localhost:5000/api/dashboard/mttr \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{}'

# Get sprints
curl -X GET http://localhost:5000/api/dashboard/sprints \
  -H "Authorization: Bearer $TOKEN"
```

### Track User Activity
```bash
TOKEN="your-jwt-token"
USER_ID="user-guid"

# Get user's audit logs
curl -X POST http://localhost:5000/api/audit/logs \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"userId\": \"$USER_ID\", \"page\": 1, \"pageSize\": 50}"

# Export user's activity
curl -X POST http://localhost:5000/api/audit/export \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"userId\": \"$USER_ID\"}" \
  --output user_activity.csv
```

---

## Testing Checklist

- [ ] Can retrieve incident metrics
- [ ] Can filter metrics by project
- [ ] Can filter metrics by date range
- [ ] Can get sprint incidents
- [ ] Can calculate MTTR
- [ ] Can view incident evolution
- [ ] Can filter audit logs by user
- [ ] Can filter audit logs by action
- [ ] Can filter audit logs by date
- [ ] Can export audit logs to CSV
- [ ] All endpoints require authentication
- [ ] Pagination works correctly
- [ ] CSV export is properly formatted

---

## Architecture Overview

```
┌─────────────────────────────────────────┐
│         WebApi/Controllers              │
│  - DashboardController                  │
│  - AuditController                      │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│         WebApi/Services                 │
│  - DashboardService                     │
│  - AuditService                         │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│      Repository/Repositories            │
│  - IncidentRepository                   │
│  - SprintRepository                     │
│  - AuditLogRepository                   │
└──────────────┬──────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│      Infrastructure/DbContext           │
│  - BugMgrDbContext                      │
│  - PostgreSQL Database                  │
└─────────────────────────────────────────┘
```

---

## Key Features

### RF4 - Dynamic Dashboards ✅
- ✅ Real-time incident metrics
- ✅ Sprint-based tracking
- ✅ Performance metrics (MTTR)
- ✅ Historical trend analysis
- ✅ Flexible filtering options

### RF5 - Audit ✅
- ✅ Automatic logging of all actions
- ✅ Comprehensive filtering
- ✅ CSV export for compliance
- ✅ User activity tracking
- ✅ Security audit trail

---

## Common Scenarios

### Scenario 1: Project Manager Dashboard
```bash
PROJECT_ID="your-project-guid"

# Get project metrics
POST /api/dashboard/metrics {"projectId": "$PROJECT_ID"}

# Get project MTTR
POST /api/dashboard/mttr {"projectId": "$PROJECT_ID"}

# Get project sprints
GET /api/dashboard/sprints?projectId=$PROJECT_ID
```

### Scenario 2: Security Audit
```bash
# Get all login attempts today
POST /api/audit/logs {
  "action": "Login",
  "startDate": "2024-01-15T00:00:00Z",
  "endDate": "2024-01-15T23:59:59Z"
}

# Export security logs
POST /api/audit/export {
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-01-31T23:59:59Z"
}
```

### Scenario 3: Performance Analysis
```bash
# Get incident evolution over last quarter
POST /api/dashboard/evolution {
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-03-31T23:59:59Z"
}

# Calculate MTTR for same period
POST /api/dashboard/mttr {
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-03-31T23:59:59Z"
}
```

---

## Support Documents

- **Complete API Documentation**: See `API_DASHBOARD_AUDIT.md`
- **Detailed Examples**: See `USAGE_EXAMPLES_DASHBOARD_AUDIT.md`
- **Implementation Details**: See `IMPLEMENTATION_RF4_RF5.md`

---

## Notes

1. All dates/times must be in ISO 8601 format (UTC)
2. All filters are optional - omit for system-wide data
3. Default date range for evolution: last 3 months
4. Default pagination: 50 items per page
5. CSV exports include all matching records (no pagination)
6. Response times typically < 3 seconds (RNF2.2)
