# Fix: UserRoles Empty in API Response

## Issue
The `/api/users` endpoint was returning users with empty `userRoles` arrays, even though users had roles assigned in the database.

## Root Cause
The `UsersController.GetUsers()` and `GetUser(id)` methods were using repository methods that didn't include the `UserRoles` navigation property:
- `GetAllAsync()` - generic method without navigation properties
- `GetAsync(id)` - generic method without navigation properties

Entity Framework doesn't lazy-load navigation properties by default, so the UserRoles collection remained empty.

## Solution
Changed the controller to use repository methods that explicitly include the navigation properties:
- `GetAllUsersWithRolesAsync()` - includes UserRoles with Role data
- `GetByIdWithRolesAsync(id)` - includes UserRoles with Role data

## Changes Made

### API/Controllers/UsersController.cs
```csharp
// Before
public async Task<ActionResult<IEnumerable<User>>> GetUsers()
{
    var users = await _userRepository.GetAllAsync();
    return Ok(users);
}

// After
public async Task<ActionResult<IEnumerable<User>>> GetUsers()
{
    var users = await _userRepository.GetAllUsersWithRolesAsync();
    return Ok(users);
}
```

```csharp
// Before
public async Task<ActionResult<User>> GetUser(Guid id)
{
    var user = await _userRepository.GetAsync(id);
    if (user == null)
    {
        return NotFound();
    }
    return Ok(user);
}

// After
public async Task<ActionResult<User>> GetUser(Guid id)
{
    var user = await _userRepository.GetByIdWithRolesAsync(id);
    if (user == null)
    {
        return NotFound();
    }
    return Ok(user);
}
```

## Example Response

### Before Fix
```json
{
  "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "name": "Administrator",
  "email": "admin@example.com",
  "username": "admin",
  "isActive": true,
  "createdAt": "2025-10-23T13:41:00+00:00",
  "updatedAt": "2025-10-23T13:41:00+00:00",
  "userRoles": []  // Empty!
}
```

### After Fix
```json
{
  "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "name": "Administrator",
  "email": "admin@example.com",
  "username": "admin",
  "isActive": true,
  "createdAt": "2025-10-23T13:41:00+00:00",
  "updatedAt": "2025-10-23T13:41:00+00:00",
  "userRoles": [
    {
      "userId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      "roleId": "a0000000-0000-0000-0000-000000000001",
      "assignedAt": "2025-10-23T13:41:00+00:00",
      "role": {
        "id": "a0000000-0000-0000-0000-000000000001",
        "code": "Admin",
        "name": "Administrador",
        "description": "Administrador del sistema con acceso total"
      }
    }
  ]
}
```

## Testing
All tests pass including new assertions to verify UserRoles are populated:
- `GetUsers_ReturnsOkWithUserList` - Verifies users list includes roles
- `GetUser_WithValidId_ReturnsOkWithUser` - Verifies single user includes roles

## Impact
This is a minimal change that fixes the issue without breaking any existing functionality. The change is backward compatible as it only adds data to the response that was previously empty.
