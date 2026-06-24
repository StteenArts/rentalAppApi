# 🏡 RentalApp - Plataforma de Reservas

Sistema backend desarrollado en ASP.NET Core + PostgreSQL + Docker que permite la gestión de propiedades, reservas, validación de identidad (KYC), sistema de notificaciones y panel de métricas para propietarios.

---

# 📌 1. Requisitos Previos

Antes de ejecutar el proyecto, asegúrate de tener instalado:

- .NET SDK 8 o superior
- Docker Desktop / Docker Engine
- Docker Compose
- Git (opcional)
- Postman o Swagger para pruebas

---

# 🚀 2. Levantar el proyecto con Docker

El proyecto está completamente containerizado con API + Base de Datos PostgreSQL.

## 📦 Paso 1: Clonar el repositorio

```bash
git clone <URL_DEL_REPOSITORIO>
cd rentalApp

```

# Paso 2: Levantar contenedores

```bash
docker-compose up --build
```

**⚙️ Servicios incluidos
API ASP.NET Core → http://localhost:5000
Swagger UI → http://localhost:5000/swagger
PostgreSQL → localhost:5432**

- Aplicar migraciones: 
*dotnet ef database update*


## Arquitectura utilizada

El proyecto fue desarrollado bajo una arquitectura monolítica modular con separación por capas:

Controllers → Services → Data (EF Core) → PostgreSQL
**Paquetes intalados en este proyecto:**

- dotnet add package Microsoft.EntityFrameworkCore
- dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
- dotnet add package Microsoft.EntityFrameworkCore.Design
- dotnet add package Microsoft.EntityFrameworkCore.Tools

- dotnet add package Swashbuckle.AspNetCore
- dotnet add package FluentValidation.AspNetCore
- dotnet add package ClosedXML

# Prevención de doble reserva (Double Booking)

Se implementó una validación de solapamiento de fechas,
Esto garantiza que no existan reservas conflictivas sobre la misma propiedad:

*(checkIn < existing.CheckOut && checkOut > existing.CheckIn)*

# Sistema de autenticación diferida

El sistema permite navegación anónima y solo solicita autenticación cuando el usuario:

Realiza una reserva
Guarda favoritos
Ejecuta acciones críticas

Esto mejora la experiencia de usuario y reduce fricción en exploración.

# KYC con cifrado de datos

Los documentos de identidad son:

Cifrados usando AES antes de almacenarse
Eliminados de memoria después del procesamiento

Esto garantiza protección de datos sensibles en reposo.

# Sistema de notificaciones omnicanal

Se implementó un motor de notificaciones capaz de:

Enviar notificaciones internas (BD)
Simular envío de correos electrónicos
Dispararse desde eventos del sistema (reservas, KYC, etc.)
# Exportación de reportes

Se implementó generación de reportes en Excel (.xlsx) usando ClosedXML, permitiendo al propietario:

Exportar reservas
Analizar ingresos
Visualizar ocupación
# Dashboard de métricas

Se expone un endpoint que calcula:

Total de propiedades
Total de reservas
Ingresos acumulados

## La solución permite:

Gestión completa de propiedades
Sistema de reservas seguro
Validación de identidad (KYC)
Notificaciones automáticas
Reportes financieros
Panel de control para propietarios


# NOTAS FINALES
El sistema fue diseñado para ser escalable, 
desacoplado y fácilmente extensible hacia una 
arquitectura de microservicios en el futuro.

Modelo realacional simple database rentalApp:
User 1 → N Property
User 1 → N Reservation
Property 1 → N Reservation
User N ↔ N Property (Wishlist)
User 1 → N Notification

