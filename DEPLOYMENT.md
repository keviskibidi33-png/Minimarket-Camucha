# Guía de Despliegue - Minimarket Camucha

## Requisitos Previos

- .NET 9.0 SDK
- SQL Server 2022 (o superior)
- Node.js 18+ y npm
- IIS o servidor web compatible (para producción)

## Configuración del Backend

### 1. Variables de Entorno

Crear `appsettings.Production.json` o configurar variables de entorno:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=MinimarketDB;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "SecretKey": "TU_CLAVE_SECRETA_MUY_LARGA_Y_SEGURA_MINIMO_32_CARACTERES",
    "Issuer": "MinimarketAPI",
    "Audience": "MinimarketWeb",
    "ExpirationMinutes": 60
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "tu-email@gmail.com",
    "SmtpPassword": "tu-app-password",
    "FromEmail": "noreply@minimarketcamucha.com",
    "FromName": "Minimarket Camucha",
    "ApiKey": "",
    "ApiUrl": "https://api.resend.com/emails"
  },
  "Cors": {
    "AllowedOrigins": ["https://tu-dominio.com"]
  },
  "FileStorage": {
    "BaseUrl": "https://api.tu-dominio.com"
  }
}
```

### 2. Configuración de Base de Datos

```bash
# Ejecutar migraciones
dotnet ef database update --project src/Minimarket.Infrastructure --startup-project src/Minimarket.API

# O si prefieres usar SQL:
# Restaurar el script de base de datos desde el proyecto
```

### 3. Compilación y Publicación

```bash
# Compilar para producción
dotnet publish src/Minimarket.API/Minimarket.API.csproj -c Release -o ./publish

# O para Linux:
dotnet publish src/Minimarket.API/Minimarket.API.csproj -c Release -r linux-x64 -o ./publish
```

### 4. Despliegue en IIS

1. Crear un sitio web en IIS
2. Configurar la carpeta de publicación como ruta física
3. Configurar el Application Pool para usar .NET CLR v4.0 o No Managed Code
4. Configurar permisos de lectura/escritura para `wwwroot/uploads`
5. Configurar SSL certificate si es HTTPS

## Configuración del Frontend

### 1. Variables de Entorno

Actualizar `src/environments/environment.prod.ts`:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.tu-dominio.com/api',
  apiTimeout: 30000
};
```

### 2. Compilación

```bash
cd minimarket-web
npm install
npm run build --configuration production
```

### 3. Despliegue

Los archivos generados estarán en `minimarket-web/dist/minimarket-web/`. Puedes:

- Desplegar en IIS (crear sitio web apuntando a esta carpeta)
- Desplegar en Nginx
- Desplegar en Apache
- Desplegar en Azure Static Web Apps, Netlify, Vercel, etc.

### 4. Configuración de Nginx (Ejemplo)

```nginx
server {
    listen 80;
    server_name tu-dominio.com;

    root /var/www/minimarket-web/dist/minimarket-web;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass https://api.tu-dominio.com;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

## Configuración de Seguridad

### 1. HTTPS

- Obtener certificado SSL (Let's Encrypt, etc.)
- Configurar redirección HTTP a HTTPS
- Configurar HSTS en producción

### 2. CORS

- Configurar solo los orígenes permitidos en `appsettings.Production.json`
- No usar `*` en producción

### 3. JWT Secret Key

- Generar una clave segura de al menos 32 caracteres
- Usar variables de entorno o Azure Key Vault en producción
- Nunca commitear la clave en el repositorio

## Monitoreo

### Health Checks

- Endpoint: `https://api.tu-dominio.com/health`
- Endpoint de readiness: `https://api.tu-dominio.com/health/ready`

### Logs

Los logs se guardan en:
- `logs/minimarket-YYYYMMDD.txt`
- Console output
- Configurar Serilog para enviar a Seq, Application Insights, etc.

## Usuarios por Defecto

Después del seed inicial:

- **Administrador**: `admin` / `Admin123!`
- **Cajero**: `cajero` / `Cajero123!`
- **Almacenero**: `almacenero` / `Almacenero123!`

**IMPORTANTE**: Cambiar las contraseñas en producción.

## Troubleshooting

### Error de conexión a base de datos

- Verificar cadena de conexión
- Verificar que SQL Server permite conexiones remotas
- Verificar firewall

### Error 404 en rutas del frontend

- Configurar servidor web para servir `index.html` en todas las rutas
- Verificar configuración de `base-href` en Angular

### Error de CORS

- Verificar que el origen del frontend está en `Cors:AllowedOrigins`
- Verificar que el backend está respondiendo con los headers CORS correctos

