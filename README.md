# Sistema E-Commerce Minimarket Camucha

Sistema de gestión y ventas para minimarket desarrollado con ASP.NET Core 9.0 y Angular 18.

## Stack Tecnológico

### Backend
- ASP.NET Core 9.0
- Entity Framework Core 9.0
- SQL Server 2022
- MediatR (CQRS)
- FluentValidation
- JWT Authentication
- Swagger/OpenAPI

### Frontend
- Angular 18+ (Standalone Components)
- Angular Material
- Tailwind CSS (complementario)
- TypeScript 5.x
- Signals (State Management)

## Estructura del Proyecto

```
MinimarketSolution.sln
├── src/
│   ├── Minimarket.API/              # Presentation Layer
│   ├── Minimarket.Application/      # Application Layer (CQRS, DTOs, Validators)
│   ├── Minimarket.Domain/           # Domain Layer (Entities, Interfaces, Enums)
│   └── Minimarket.Infrastructure/   # Infrastructure Layer (Data, Repositories)
└── minimarket-web/                  # Frontend Angular
```

## Instalación y Configuración

### Requisitos Previos
- .NET 9.0 SDK
- Node.js 18+ y npm
- SQL Server 2022 (o superior)
- Visual Studio 2022 o VS Code

### Backend

1. Restaurar dependencias:
```bash
dotnet restore
```

2. Actualizar connection string en `src/Minimarket.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MinimarketDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

3. Crear migración inicial:
```bash
cd src/Minimarket.API
dotnet ef migrations add InitialCreate --project ../Minimarket.Infrastructure --startup-project .
```

4. Aplicar migraciones (se ejecuta automáticamente al iniciar):
```bash
dotnet ef database update --project ../Minimarket.Infrastructure --startup-project .
```

5. Ejecutar la API:
```bash
dotnet run --project src/Minimarket.API
```

La API estará disponible en:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger: https://localhost:5001/swagger

### Frontend

1. Instalar dependencias:
```bash
cd minimarket-web
npm install
```

2. Ejecutar en desarrollo:
```bash
npm start
```

La aplicación estará disponible en: http://localhost:4200

## Usuarios de Prueba

El sistema crea automáticamente los siguientes usuarios al iniciar:

- **Administrador**
  - Usuario: `admin`
  - Contraseña: `Admin123!`
  - Rol: Administrador

- **Cajero**
  - Usuario: `cajero`
  - Contraseña: `Cajero123!`
  - Rol: Cajero

- **Almacenero**
  - Usuario: `almacenero`
  - Contraseña: `Almacenero123!`
  - Rol: Almacenero

## Datos Iniciales

El sistema incluye seeders automáticos que crean:
- 6 categorías (Lácteos, Abarrotes, Bebidas, Golosinas, Conservas, Limpieza)
- 50 productos de ejemplo
- 10 clientes de prueba

## API Endpoints

### Autenticación
- `POST /api/auth/login` - Iniciar sesión

## Características Implementadas (Fase 1)

✅ Estructura Clean Architecture
✅ Entity Framework Core con SQL Server
✅ JWT Authentication
✅ CQRS con MediatR
✅ FluentValidation
✅ Repository Pattern + Unit of Work
✅ Swagger/OpenAPI
✅ Angular 18 con Standalone Components
✅ Angular Material + Tailwind CSS
✅ Layout responsive con sidebar
✅ Sistema de autenticación frontend
✅ Guards y Interceptors
✅ Seeders de datos iniciales

## Próximos Pasos (Fase 2)

- Módulo de Productos (CRUD completo)
- Módulo de Categorías
- Módulo de Clientes
- Punto de Venta (POS)
- Gestión de Inventario
- Reportes y Dashboard

## Notas

- Los diseños del frontend replican fielmente los HTML proporcionados
- Colores primarios: Verde (#4CAF50) para tienda, Azul (#0d7ff2) para admin
- Dark mode implementado
- Material Symbols para iconos

## Características de Producción

✅ Control de acceso por roles (Administrador, Cajero, Almacenero)
✅ Autorización basada en roles en backend y frontend
✅ Paginación real en base de datos (no en memoria)
✅ Upload de imágenes con validación
✅ Páginas de error 404 y 500
✅ Health checks para monitoreo
✅ Middleware de seguridad (headers, HSTS)
✅ Configuración de producción con variables de entorno
✅ CORS configurable
✅ Swagger solo en desarrollo

## Despliegue

Para información detallada sobre el despliegue en producción, consulta [DEPLOYMENT.md](DEPLOYMENT.md).

### Variables de Entorno Importantes

Configura las siguientes variables en producción:
- Connection String (Base de datos)
- JWT Secret Key (mínimo 32 caracteres)
- Email Settings (SMTP o API)
- CORS Allowed Origins
- File Storage Base URL

Ver `.env.example` para un ejemplo completo.

## Licencia

Este proyecto es privado y confidencial.

