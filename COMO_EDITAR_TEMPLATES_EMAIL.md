# Cómo Editar los Templates de Email

## Ubicación de los Templates

Los templates de email están en el archivo:
**`src/Minimarket.Infrastructure/Services/EmailService.cs`**

## Templates Disponibles

### 1. Template de Confirmación de Pedido
**Método:** `SendOrderConfirmationAsync()`
**Líneas:** ~93-142

**Cuándo se envía:** Cuando un cliente confirma un pedido

**Variables disponibles:**
- `{customerName}` - Nombre del cliente
- `{orderNumber}` - Número de pedido
- `{total}` - Total del pedido (formato: S/ XX.XX)
- `{shippingMethodText}` - "Despacho a Domicilio" o "Retiro en Tienda"
- `{deliveryText}` - Fecha estimada de entrega o mensaje

**Ejemplo de edición:**
```csharp
var body = $@"
    <html>
    <body style='font-family: Arial, sans-serif;'>
        <h1>¡Pedido Confirmado!</h1>
        <p>Estimado/a <strong>{customerName}</strong>,</p>
        <p>Tu pedido #{orderNumber} ha sido confirmado.</p>
        <!-- Edita aquí el contenido -->
    </body>
    </html>";
```

---

### 2. Template de Actualización de Estado
**Método:** `SendOrderStatusUpdateAsync()`
**Líneas:** ~144-202

**Cuándo se envía:** Cuando cambia el estado del pedido (preparando, despachado, entregado, etc.)

**Variables disponibles:**
- `{customerName}` - Nombre del cliente
- `{orderNumber}` - Número de pedido
- `{statusText}` - Texto del estado (ej: "Preparando tu pedido")
- `{trackingUrl}` - URL de seguimiento (opcional)

**Estados disponibles:**
- `"preparing"` → "Preparando tu pedido"
- `"shipped"` → "Tu pedido ha sido despachado"
- `"delivered"` → "Tu pedido ha sido entregado"
- `"ready_for_pickup"` → "Tu pedido está listo para retiro"

---

### 3. Template de Recibo de Venta (POS)
**Método:** `SendSaleReceiptAsync()`
**Líneas:** ~73-91

**Cuándo se envía:** Cuando se envía un recibo desde el POS

**Variables disponibles:**
- `{customerName}` - Nombre del cliente
- `{saleNumber}` - Número de venta
- `{documentTypeText}` - "Factura" o "Boleta"
- Incluye PDF adjunto

---

## Cómo Editar

### Paso 1: Abrir el archivo
```
src/Minimarket.Infrastructure/Services/EmailService.cs
```

### Paso 2: Localizar el método
- Busca el método del template que quieres editar
- Los templates están en strings con `$@"..."` (interpolación de strings)

### Paso 3: Editar el HTML
- Puedes cambiar colores, textos, estilos, estructura
- Usa las variables disponibles con `{variable}`
- Mantén el formato HTML válido

### Paso 4: Guardar y recompilar
- Guarda el archivo
- El backend se recompilará automáticamente (si está en modo watch)
- O reinicia el backend manualmente

---

## Ejemplo: Cambiar Colores

**Antes:**
```csharp
<div style='background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%);'>
```

**Después (color verde):**
```csharp
<div style='background: linear-gradient(135deg, #10b981 0%, #059669 100%);'>
```

---

## Ejemplo: Agregar Logo

```csharp
<div style='text-align: center; margin-bottom: 20px;'>
    <img src='https://tudominio.com/logo.png' alt='Minimarket Camucha' style='max-width: 200px;'>
</div>
```

---

## Ejemplo: Cambiar Textos

**Antes:**
```csharp
<p>Gracias por tu compra. Tu pedido <strong>#{orderNumber}</strong> ha sido confirmado.</p>
```

**Después:**
```csharp
<p>¡Gracias por confiar en nosotros! Tu pedido <strong>#{orderNumber}</strong> está siendo preparado con mucho cuidado.</p>
```

---

## Variables Especiales

### Fechas
```csharp
{estimatedDelivery.Value:dd 'de' MMMM, yyyy}  // Ej: "15 de enero, 2025"
{estimatedDelivery.Value:dd/MM/yyyy}          // Ej: "15/01/2025"
```

### Precios
```csharp
{total:F2}  // Ej: "150.50" → "150.50"
{total:C}   // Ej: "150.50" → "S/ 150.50" (formato de moneda)
```

---

## Tips de Diseño

1. **Responsive:** Los emails se ven en móviles, usa `max-width: 600px`
2. **Colores:** Usa los colores de tu marca
3. **Fuentes:** Arial, sans-serif es seguro para todos los clientes
4. **Imágenes:** Usa URLs absolutas, no rutas relativas
5. **Estilos inline:** Muchos clientes de email no soportan CSS externo

---

## Probar los Cambios

1. Edita el template
2. Guarda el archivo
3. Reinicia el backend
4. Completa un pedido desde el frontend
5. Revisa el correo recibido

---

## Nota Importante

⚠️ **Después de editar, siempre:**
- Guarda el archivo
- Verifica que compile sin errores
- Prueba enviando un email real
- Revisa cómo se ve en diferentes clientes de email (Gmail, Outlook, etc.)

