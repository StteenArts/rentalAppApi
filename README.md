# 🏡 RentalApp API

Backend REST puro en ASP.NET Core (.NET 10) + PostgreSQL para una plataforma de rentas cortas tipo Airbnb: propiedades, reservas, favoritos, verificación de identidad (KYC, simulada), notificaciones (simuladas) y dashboards con export a Excel.

Arquitectura: monolito modular por capas `Controllers → Services → Data (EF Core) → PostgreSQL`. Sin frontend propio: pensado para que un equipo de frontend consuma la API directamente.

---

## 1. Requisitos previos

- Docker + Docker Compose
- (Opcional, para desarrollo local sin Docker) .NET SDK 10

---

## 2. Configuración de secretos

Ningún secreto (password de Postgres, clave de firma JWT, clave AES de cifrado KYC) está hardcodeado en el código ni en `appsettings.json`.

### Con Docker

```bash
cp .env.example .env
# edita .env con valores propios si quieres (los de ejemplo funcionan para desarrollo)
docker compose up --build
```

### Desarrollo local sin Docker

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=RentalAppDb;Username=postgres;Password=postgres"
dotnet user-secrets set "Jwt:Key" "un-secreto-largo-de-desarrollo"
dotnet user-secrets set "Crypto:AesKey" "12345678901234567890123456789012"
dotnet run
```

Las migraciones se aplican automáticamente al iniciar la aplicación (`db.Database.Migrate()` en `Program.cs`), no se requieren pasos manuales.

---

## 3. Servicios expuestos (Docker)

| Servicio | URL |
|---|---|
| API | http://localhost:5000 |
| Swagger UI (solo en Development) | http://localhost:5000/swagger |
| PostgreSQL | localhost:5432 |

---

## 4. Autenticación y roles

JWT propio (email + password, sin login social). Roles: `Guest`, `Owner`, `Admin`.

- El registro público solo permite `Guest` u `Owner` (rol `Admin` se provisiona manualmente en la base de datos, no vía self-registration).
- Todos los endpoints requieren un Bearer token **por defecto**, salvo los explícitamente públicos (`[AllowAnonymous]`): listar/ver/buscar propiedades.
- El id del usuario autenticado siempre se toma de los claims del JWT, nunca de parámetros enviados por el cliente.

```
POST /api/auth/register   Body: { fullName, email, password, role? }   -> 201 { id, fullName, email, role }
POST /api/auth/login      Body: { email, password }                   -> 200 { token, expiresAt, user }
GET  /api/auth/me         [Authorize]                                 -> 200 { id, fullName, email, role }
```

---

## 5. Endpoints principales

```
api/auth            register, login, me
api/properties       GET (público), GET/{id} (público), GET/search (público),
                      GET/mine [Owner], POST [Owner|Admin], PUT/{id}, DELETE/{id} (dueño o Admin),
                      POST/{id}/images, DELETE/{id}/images/{imageId} (dueño o Admin)
api/reservations     POST (crea, exige KYC aprobado), GET/my, GET/owner [Owner|Admin], POST/{id}/cancel
api/wishlist         POST/toggle, GET, DELETE/{propertyId}
api/kyc              POST/upload, GET/me, DELETE/{id}
api/notifications    GET (mías), PUT/read/{id}
api/dashboard        GET/metrics [Owner|Admin] (?startDate&endDate), GET/export [Owner|Admin] (?propertyId&startDate&endDate)
```

Todas las respuestas de error usan `ProblemDetails` (JSON consistente), nunca stack traces crudos.

---

## 6. Reglas de negocio implementadas

- **No double-booking**: una propiedad no puede tener dos reservas activas con fechas solapadas.
- **Horarios fijos**: check-in 2:00 PM, check-out 12:00 PM, sin importar lo que envíe el cliente.
- **Navegación anónima**: catálogo y búsqueda de propiedades no requieren login; reservar, pagar o guardar favoritos sí.
- **KYC obligatorio**: no se puede reservar sin al menos un documento KYC con estado `Approved`.
- **KYC cifrado**: los documentos se cifran con AES antes de guardarse y se sobrescriben en memoria/DB al eliminarse.
- **CheckIn no puede ser pasado**: `CreateReservationDtoValidator` rechaza reservas con `CheckIn` anterior a la fecha actual (UTC).

---

## 7. Funcionalidades simuladas (marcadas intencionalmente, no son bugs)

- **Veredicto KYC**: `Services/KycService.cs` simula una respuesta de OCR/IA (aprueba/rechaza según extensión y tamaño de archivo, y genera datos de identidad ficticios). Preparado para reemplazarse por una integración real sin tocar el resto del flujo (`KycController` ya llama a esta pieza de forma aislada).
- **Notificaciones**: `Services/EmailService.cs` imprime el "envío" de correo por consola; `Services/NotificationService.cs` sí persiste la notificación in-app real en la base de datos.
- **Pagos**: el modelo `Payment` existe en el dominio pero la integración con una pasarela real no está implementada (fuera del alcance de esta pasada de correcciones).
- **Almacenamiento de imágenes**: las imágenes de propiedades y los documentos KYC se guardan en disco local (`wwwroot/uploads` y la base de datos, respectivamente), no en un bucket S3.

---

## 8. Notas técnicas relevantes

- Passwords con BCrypt (no SHA-256).
- CORS configurable vía `Cors:AllowedOrigins` en `appsettings.json`.
- FluentValidation registrado y activo (`Program.cs` + `Models/Dtos/Validators/`).
- Swagger solo disponible en `Development`.
- `.dockerignore` excluye `bin/`, `obj/`, `.git/`, `.idea/`, `.vs/`, `.env` y `*.user` del build context de Docker.
- Proyecto es Web API pura: no incluye el scaffold MVC de vistas Razor (`HomeController`/`Views`) que traía la plantilla por defecto.
