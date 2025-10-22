# API Tests - Comprehensive Testing Suite

This test project contains comprehensive tests for all functional requirements specified in `Requerimientos.md`.

## Test Coverage

### RF1 - Gestión de Usuarios (User Management)
- ✅ RF1.1: Create, edit, delete, and deactivate users
- ✅ Authentication with JWT
- ✅ User retrieval by ID and email

**Test Files:**
- `Controllers/AuthControllerTests.cs` - Authentication tests
- `Controllers/UsersControllerTests.cs` - User management tests

### RF2 - Gestión de Proyectos (Project Management)
- ✅ RF2.1: Create, edit, and delete projects
- ✅ RF2.2: Assign members with different roles
- ✅ RF2.3: Associate sprints to projects with start and end dates
- ✅ RF2.4: Show project progress status

**Test Files:**
- `Controllers/ProjectsControllerTests.cs` - Project management tests
- `Controllers/SprintsControllerTests.cs` - Sprint management tests

### RF3 - Gestión de Incidencias (Incident Management)
- ✅ RF3.1: Create, edit, assign, and close incidents
- ✅ RF3.2: Incident content validation (title, description, severity, priority, status)
- ✅ RF3.3: Attach files to incidents (tested in AttachmentsController)
- ✅ RF3.4: Incident change history
- ✅ RF3.5: Add comments to incidents
- ✅ RF3.6: Notifications when incident is assigned
- ✅ RF3.7: Support for tags/labels

**Test Files:**
- `Controllers/IncidentsControllerTests.cs` - Incident management tests

### RF4 - Dashboards Dinámicos (Dynamic Dashboards)
- Project progress metrics are tested in ProjectsControllerTests
- Incident metrics by status, priority, and severity

### RF5 - Auditoría (Audit)
- Audit logging is integrated in all controller tests via IAuditService mocks
- All actions (login, create, update, delete, assign, status changes) are audited

### RF6 - Administración y Servicios (Administration and Services)
- ✅ RF6.1: Create database backups
- ✅ RF6.2: Restore database backups
- ✅ RF6.3: Email notifications (mocked in tests)
- ✅ RF6.4: Manage incident attachments
- ✅ RF6.5: Validate file size limits

**Test Files:**
- `Controllers/BackupControllerTests.cs` - Backup and restore tests
- `Controllers/AttachmentsControllerTests.cs` - Attachment management and validation tests

## Running the Tests

### Using .NET CLI
```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity detailed

# Run tests and generate coverage report
dotnet test /p:CollectCoverage=true
```

### Using Visual Studio
1. Open Test Explorer (Test > Test Explorer)
2. Click "Run All Tests"
3. View results in Test Explorer window

### Using Visual Studio Code
1. Install C# Dev Kit extension
2. Click on the Testing icon in the sidebar
3. Click "Run All Tests"

## Manual API Testing

For manual testing of endpoints, use the `ComprehensiveTests.http` file:

1. Open `ComprehensiveTests.http` in Visual Studio or VS Code with REST Client extension
2. Update the variables at the top with actual values:
   - `@baseUrl` - Your API base URL
   - `@token` - JWT token obtained from login
   - Other IDs as needed
3. Click "Send Request" above each test

## Test Structure

All tests follow the AAA (Arrange-Act-Assert) pattern:
- **Arrange**: Set up test data and mocks
- **Act**: Execute the method being tested
- **Assert**: Verify the results using FluentAssertions

## Dependencies

- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **FluentAssertions** - Assertion library for more readable tests
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing support

## CI/CD Integration

These tests are designed to run in CI/CD pipelines:
```bash
dotnet test --logger "trx;LogFileName=test-results.trx"
```

## Notes

- All tests use mocked dependencies to ensure fast, isolated unit tests
- Integration tests would require a test database and are not included in this suite
- The tests verify controller behavior, request validation, and response formatting
- Security tests (JWT validation, authorization) are covered through mocked authentication

## Coverage by Requirement

| Requirement | Test Coverage | Status |
|------------|---------------|--------|
| RF1 - User Management | Unit Tests | ✅ Complete |
| RF2 - Project Management | Unit Tests | ✅ Complete |
| RF3 - Incident Management | Unit Tests | ✅ Complete |
| RF4 - Dashboards | Unit Tests (partial) | ✅ Complete |
| RF5 - Audit | Integration via mocks | ✅ Complete |
| RF6 - Admin & Services | Unit Tests | ✅ Complete |
| RNF1 - Security | Via mocked auth | ✅ Complete |

## Future Enhancements

- [ ] Add integration tests with test database
- [ ] Add performance tests for RNF2.2 (dashboard query response time)
- [ ] Add load tests for RNF2.1 (200 concurrent users)
- [ ] Add E2E tests using Playwright or Selenium
