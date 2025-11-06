# Cómo Usar los Templates de Email

## Estructura Simplificada

Los templates de email ahora tienen un diseño más simple y directo:

```
┌─────────────────────────────────────┐
│  [Logo - Arriba]                    │
│  ¡Pedido Confirmado!                │
├─────────────────────────────────────┤
│  [Cuerpo Blanco]                     │
│  Estimado/a [Nombre],                │
│  Gracias por tu compra...           │
│                                     │
│  ┌─────────────────────────────┐   │
│  │ Detalles del Pedido          │   │
│  │ Método: Despacho/Retiro      │   │
│  │ Total: S/ 150.00             │   │
│  │ Fecha: 15 de enero, 2025    │   │
│  └─────────────────────────────┘   │
│                                     │
│  Recibirás notificaciones...        │
├─────────────────────────────────────┤
│  [Imagen de Promoción]              │
├─────────────────────────────────────┤
│  Minimarket Camucha                  │
│  Correo automático                  │
└─────────────────────────────────────┘
```

## Cómo Subir Imágenes

### 1. Colocar las imágenes en el servidor

Crea la carpeta `wwwroot/email-templates/` en el proyecto API:

```
src/Minimarket.API/wwwroot/email-templates/
├── logo.png              # Logo de Minimarket Camucha
└── promotion.png         # Imagen de promoción
```

### 2. URLs de las imágenes

**En desarrollo:**
- Logo: `http://localhost:5000/email-templates/logo.png`
- Promoción: `http://localhost:5000/email-templates/promotion.png`

**En producción:**
- Logo: `https://tudominio.com/email-templates/logo.png`
- Promoción: `https://tudominio.com/email-templates/promotion.png`

## Configurar desde el Admin Panel

1. Ve a **Configuraciones** → **Templates de Email**
2. Ingresa las URLs de las imágenes:
   - **URL del Logo**: URL completa del logo (arriba del email)
   - **URL de Promoción**: URL completa de la imagen de promoción (abajo del email)
3. Haz clic en **Guardar Configuración**

### Vista Previa

El admin panel muestra una vista previa de las imágenes mientras las configuras, para verificar que las URLs sean correctas.

## Templates Disponibles

### 1. Confirmación de Pedido (`order_confirmation`)
- Se envía cuando un cliente confirma un pedido
- Incluye: Logo, título, detalles del pedido, imagen de promoción

### 2. Actualización de Estado (`order_status_update`)
- Se envía cuando cambia el estado del pedido
- Incluye: Logo, título, estado actual, botón de rastreo (opcional), imagen de promoción

## Personalización

### Cambiar el Logo

1. Reemplaza `logo.png` en `wwwroot/email-templates/`
2. O actualiza la URL en el admin panel si usas una URL externa

### Cambiar la Imagen de Promoción

1. Reemplaza `promotion.png` en `wwwroot/email-templates/`
2. O actualiza la URL en el admin panel si usas una URL externa

### Editar el Contenido HTML

Los templates están en:
- `src/Minimarket.Infrastructure/Services/EmailService.cs`
- Métodos: `SendOrderConfirmationAsync()` y `SendOrderStatusUpdateAsync()`

**Nota:** Los cambios en el código requieren recompilar la aplicación.

## Persistencia

Las URLs configuradas desde el admin panel se guardan en la base de datos en la tabla `SystemSettings`:
- `email_template_logo_url`
- `email_template_promotion_url`

Si no están configuradas, se usan los valores por defecto desde `appsettings.json` o las rutas locales.

## Troubleshooting

### Las imágenes no se muestran

1. Verifica que las URLs sean accesibles públicamente
2. Asegúrate de que el servidor esté sirviendo archivos estáticos desde `wwwroot`
3. Verifica que las rutas sean correctas (sin espacios, caracteres especiales, etc.)

### Error 404 en las imágenes

- Verifica que la carpeta `wwwroot/email-templates/` exista
- Verifica que los archivos tengan los nombres correctos (`logo.png`, `promotion.png`)
- Reinicia el servidor después de agregar nuevos archivos

### Las imágenes no se actualizan

- Limpia la caché del navegador
- Verifica que las URLs en el admin panel sean correctas
- Asegúrate de haber guardado los cambios en el admin panel

