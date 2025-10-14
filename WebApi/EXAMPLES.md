# Ejemplos de Uso de la API

Este documento contiene ejemplos prácticos de cómo usar los endpoints de autenticación y autorización.

## Requisitos

- La API debe estar ejecutándose (por defecto en `http://localhost:5000`)
- Los roles deben estar insertados en la base de datos (ver `Scripts/seed-roles.sql`)

## Ejemplos con cURL

### 1. Registrar un Nuevo Usuario

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "María García",
    "email": "maria.garcia@example.com",
    "username": "mgarcia",
    "password": "SecurePass123"
  }'
```

**Respuesta esperada:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "mgarcia",
  "email": "maria.garcia@example.com",
  "roles": ["Developer"],
  "expiresAt": "2025-10-14T01:34:23.978Z"
}
```

### 2. Iniciar Sesión

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "mgarcia",
    "password": "SecurePass123"
  }'
```

**Respuesta esperada:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "mgarcia",
  "email": "maria.garcia@example.com",
  "roles": ["Developer"],
  "expiresAt": "2025-10-14T01:34:23.978Z"
}
```

**Nota:** Guardar el token de la respuesta para usarlo en los siguientes requests.

### 3. Obtener Información del Usuario Actual

```bash
# Reemplazar YOUR_TOKEN con el token obtenido en login
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X GET http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer $TOKEN"
```

**Respuesta esperada:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "mgarcia",
  "roles": ["Developer"]
}
```

### 4. Acceder a Endpoint Público (sin autenticación)

```bash
curl -X GET http://localhost:5000/api/test/public
```

**Respuesta esperada:**
```json
{
  "message": "Este endpoint es público y no requiere autenticación"
}
```

### 5. Acceder a Endpoint Protegido (requiere autenticación)

```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X GET http://localhost:5000/api/test/protected \
  -H "Authorization: Bearer $TOKEN"
```

**Respuesta esperada:**
```json
{
  "message": "Este endpoint requiere autenticación",
  "user": "mgarcia"
}
```

### 6. Intentar Acceder a Endpoint de Admin (será rechazado si no es admin)

```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X GET http://localhost:5000/api/test/admin-only \
  -H "Authorization: Bearer $TOKEN"
```

**Respuesta si no es admin (403 Forbidden):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403
}
```

## Ejemplos con JavaScript (Fetch API)

### Login y Guardar Token

```javascript
async function login(username, password) {
  const response = await fetch('http://localhost:5000/api/auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ username, password })
  });
  
  if (!response.ok) {
    throw new Error('Login failed');
  }
  
  const data = await response.json();
  // Guardar token en localStorage
  localStorage.setItem('token', data.token);
  localStorage.setItem('username', data.username);
  localStorage.setItem('roles', JSON.stringify(data.roles));
  
  return data;
}

// Uso
login('mgarcia', 'SecurePass123')
  .then(data => console.log('Login exitoso:', data))
  .catch(error => console.error('Error:', error));
```

### Hacer Request con Token

```javascript
async function getProtectedData() {
  const token = localStorage.getItem('token');
  
  const response = await fetch('http://localhost:5000/api/test/protected', {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  if (!response.ok) {
    if (response.status === 401) {
      // Token expirado o inválido, hacer logout
      logout();
      throw new Error('Por favor inicie sesión nuevamente');
    }
    throw new Error('Request failed');
  }
  
  return await response.json();
}

function logout() {
  localStorage.removeItem('token');
  localStorage.removeItem('username');
  localStorage.removeItem('roles');
}
```

### Verificar Roles

```javascript
function hasRole(requiredRole) {
  const roles = JSON.parse(localStorage.getItem('roles') || '[]');
  return roles.includes(requiredRole);
}

function hasAnyRole(...requiredRoles) {
  const roles = JSON.parse(localStorage.getItem('roles') || '[]');
  return requiredRoles.some(role => roles.includes(role));
}

// Uso
if (hasRole('Admin')) {
  console.log('Usuario es administrador');
}

if (hasAnyRole('Admin', 'ProductOwner')) {
  console.log('Usuario puede gestionar proyectos');
}
```

## Ejemplos con Angular

### Auth Service

```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  username: string;
  email: string;
  roles: string[];
  expiresAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/auth';
  private currentUserSubject = new BehaviorSubject<AuthResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    // Cargar usuario desde localStorage al iniciar
    const storedUser = localStorage.getItem('currentUser');
    if (storedUser) {
      this.currentUserSubject.next(JSON.parse(storedUser));
    }
  }

  login(username: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { username, password })
      .pipe(
        tap(response => {
          localStorage.setItem('currentUser', JSON.stringify(response));
          localStorage.setItem('token', response.token);
          this.currentUserSubject.next(response);
        })
      );
  }

  logout(): void {
    localStorage.removeItem('currentUser');
    localStorage.removeItem('token');
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  hasRole(role: string): boolean {
    const user = this.currentUserSubject.value;
    return user?.roles.includes(role) || false;
  }

  hasAnyRole(...roles: string[]): boolean {
    const user = this.currentUserSubject.value;
    return roles.some(role => user?.roles.includes(role)) || false;
  }
}
```

### HTTP Interceptor para JWT

```typescript
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getToken();
    
    if (token) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
    
    return next.handle(request);
  }
}
```

### Auth Guard

```typescript
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (this.authService.isAuthenticated()) {
      const requiredRoles = route.data['roles'] as string[];
      
      if (requiredRoles && requiredRoles.length > 0) {
        if (this.authService.hasAnyRole(...requiredRoles)) {
          return true;
        } else {
          this.router.navigate(['/forbidden']);
          return false;
        }
      }
      
      return true;
    }

    this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }
}
```

### Uso en Rutas

```typescript
const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { 
    path: 'dashboard', 
    component: DashboardComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'admin', 
    component: AdminComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Admin'] }
  },
  { 
    path: 'projects', 
    component: ProjectsComponent,
    canActivate: [AuthGuard],
    data: { roles: ['Admin', 'ProductOwner'] }
  }
];
```

## Creación de Usuario Administrador (SQL Directo)

Para crear el primer usuario administrador, ejecutar este SQL directamente en la base de datos:

```sql
-- 1. Insertar el usuario
INSERT INTO "Users" ("Id", "Name", "Email", "Username", "PasswordHash", "IsActive", "CreatedAt", "UpdatedAt")
VALUES (
    gen_random_uuid(),
    'Administrador',
    'admin@example.com',
    'admin',
    '$2a$11$K7GJsn.H7x9WZLYPqQQrS.8vGJKLJ.BHc6rJ7/9.8yKHc6rJ7/9.8', -- password: admin123
    true,
    NOW(),
    NOW()
);

-- 2. Obtener el ID del usuario recién creado
-- Guardarlo para el siguiente paso

-- 3. Asignar rol de Admin
INSERT INTO "UserRoles" ("UserId", "RoleId", "AssignedAt")
SELECT 
    u."Id",
    r."Id",
    NOW()
FROM "Users" u, "Roles" r
WHERE u."Username" = 'admin' AND r."Code" = 'Admin';
```

**Nota:** Para generar un hash de contraseña válido, usar el endpoint de registro o una herramienta de BCrypt.

## Códigos de Estado HTTP

| Código | Significado | Cuándo Ocurre |
|--------|-------------|---------------|
| 200 | OK | Request exitoso |
| 400 | Bad Request | Datos de entrada inválidos |
| 401 | Unauthorized | Token inválido o ausente |
| 403 | Forbidden | Usuario autenticado pero sin permisos |
| 500 | Internal Server Error | Error en el servidor |

## Errores Comunes

### 401 Unauthorized - "Token inválido"

**Causa:** Token expirado o mal formado

**Solución:** Hacer login nuevamente para obtener un nuevo token

### 403 Forbidden

**Causa:** Usuario autenticado pero no tiene el rol requerido

**Solución:** Verificar que el usuario tiene los roles apropiados en la base de datos

### 400 Bad Request - "Username or email already exists"

**Causa:** Intentando registrar un usuario con username o email que ya existe

**Solución:** Usar credenciales diferentes

## Pruebas Recomendadas

1. **Registrar un usuario** → Verificar que se crea con rol "Developer"
2. **Iniciar sesión** → Verificar que se recibe un token válido
3. **Acceder a /api/auth/me** → Verificar que devuelve información del usuario
4. **Acceder a endpoint protegido sin token** → Verificar 401
5. **Acceder a endpoint de admin sin ser admin** → Verificar 403
6. **Esperar 60 minutos** → Verificar que el token expira y devuelve 401

## Notas de Seguridad

- **Nunca** compartir tokens JWT
- **Nunca** almacenar contraseñas en texto plano
- **Siempre** usar HTTPS en producción
- **Siempre** cambiar la SecretKey en producción
- **Rotar** la SecretKey periódicamente
- **Implementar** rate limiting para prevenir ataques de fuerza bruta
