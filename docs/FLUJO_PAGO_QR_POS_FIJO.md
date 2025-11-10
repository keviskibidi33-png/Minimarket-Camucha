# Flujo de Pago QR - POS Fijo (Pantalla Táctil Inamovible)

## 🖥️ Contexto
- **POS**: Pantalla táctil fija/inamovible (no se puede girar)
- **Celular del negocio**: Opcional, para mostrar QR al cliente
- **QR físico**: Impreso en repisa/mostrador (alternativa)

## 🔄 Flujo Ajustado

### **Opción A: Usando Celular del Negocio**

```
1. Cliente selecciona productos → Carrito en POS
   ↓
2. Cajero selecciona "Yape/Plin" en POS
   ↓
3. POS genera QR y muestra en pantalla:
   ┌─────────────────────────────┐
   │  Pago con Yape/Plin         │
   │  ────────────────────────   │
   │  [QR CODE]                   │
   │  Teléfono: 999 888 777      │
   │  Monto: S/ 150.00           │
   │  Ref: V001-000123           │
   │  [Copiar Teléfono]          │
   │  [Abrir en Celular] ← NUEVO │
   └─────────────────────────────┘
   ↓
4. Cajero toma celular del negocio
   ↓
5. Cajero presiona "Abrir en Celular"
   → Se abre QR en navegador del celular
   → O se envía por WhatsApp/email al celular
   ↓
6. Cajero muestra celular al cliente
   ↓
7. Cliente escanea QR del celular
   ↓
8. Cliente paga en su app Yape/Plin
   ↓
9. Cliente muestra confirmación al cajero
   ↓
10. Cajero vuelve al POS y presiona "Cobrar"
    ↓
11. ✅ Venta procesada
    ✅ Boleta generada
```

### **Opción B: Usando QR Físico Impreso**

```
1. Cliente selecciona productos → Carrito en POS
   ↓
2. Cajero selecciona "Yape/Plin" en POS
   ↓
3. POS muestra:
   ┌─────────────────────────────┐
   │  Pago con Yape/Plin         │
   │  ────────────────────────   │
   │  Monto: S/ 150.00           │
   │  Ref: V001-000123           │
   │  Teléfono: 999 888 777      │
   │                             │
   │  [Mostrar QR Físico]        │
   │  [Copiar Teléfono]          │
   │                             │
   │  [Cobrar] ← Habilitado      │
   └─────────────────────────────┘
   ↓
4. Cajero indica al cliente:
   "Puede escanear el QR de la repisa"
   O "Puede pagar al número 999 888 777"
   ↓
5. Cliente escanea QR físico O paga manualmente
   ↓
6. Cliente paga en su app
   ↓
7. Cliente muestra confirmación al cajero
   ↓
8. Cajero presiona "Cobrar" en POS
   ↓
9. ✅ Venta procesada
```

## 📱 Funcionalidades del POS

### Cuando se selecciona Yape/Plin:

1. **Mostrar información de pago**:
   - Monto total
   - Número de teléfono (Yape/Plin)
   - Referencia de venta
   - Botón "Copiar Teléfono"

2. **Opciones según disponibilidad**:
   - **Si hay celular**: Botón "Abrir QR en Celular"
   - **Si hay QR físico**: Mensaje "Escanee el QR de la repisa"
   - **Siempre disponible**: Botón "Copiar Teléfono" (para pago manual)

3. **Botón "Cobrar"**:
   - Siempre visible y habilitado
   - Cajero presiona después de verificar pago
   - Procesa la venta normalmente

## 🎯 Características Clave

### 1. **QR en Celular del Negocio**
- Opción 1: Generar link que se abre en navegador del celular
- Opción 2: Enviar QR por WhatsApp al celular del negocio
- Opción 3: Mostrar código para escanear desde el celular

### 2. **QR Físico**
- QR pre-impreso en repisa/mostrador
- Contiene número de teléfono fijo
- Cliente escanea y paga monto manualmente
- Cajero valida monto en confirmación

### 3. **Pago Manual (Sin QR)**
- Mostrar número de teléfono grande
- Botón "Copiar Teléfono"
- Cliente ingresa número manualmente en su app
- Cliente paga monto mostrado

## 💡 Implementación Propuesta

### En el POS:

```typescript
// Cuando se selecciona Yape/Plin
if (paymentMethod === 'YapePlin') {
  // Generar referencia única
  const reference = generateSaleReference();
  
  // Mostrar panel de pago QR
  showQRPaymentPanel({
    amount: total,
    phoneNumber: brandSettings.yapePhone, // o plinPhone
    reference: reference,
    options: {
      showQRForMobile: true,  // Si hay celular disponible
      showPhysicalQR: true,  // Si hay QR físico
      showManualPayment: true // Siempre disponible
    }
  });
}
```

### Panel de Pago QR:

```
┌─────────────────────────────────────┐
│  Pago con Yape/Plin                 │
│  ─────────────────────────────────  │
│                                      │
│  Monto a Pagar:                      │
│  S/ 150.00                           │
│                                      │
│  Teléfono: 999 888 777              │
│  [📋 Copiar]                         │
│                                      │
│  Referencia: V001-000123            │
│                                      │
│  ┌─────────────────────────────┐    │
│  │  [QR CODE]                  │    │
│  │  (Para mostrar en celular)  │    │
│  └─────────────────────────────┘    │
│  [📱 Abrir en Celular]               │
│                                      │
│  O escanee el QR de la repisa        │
│                                      │
│  [❌ Cancelar]  [✅ Cobrar]          │
└─────────────────────────────────────┘
```

## 🔧 Componentes a Crear

1. **QRPaymentPanel Component**
   - Muestra información de pago
   - Genera QR code
   - Botones de acción

2. **QR Generator Service**
   - Genera QR con formato estándar
   - Opción de exportar a imagen
   - Opción de generar link

3. **Mobile QR Handler**
   - Abre QR en navegador móvil
   - O envía por WhatsApp
   - O genera código para escanear

## 📋 Configuración Necesaria

En BrandSettings:
- `YapePhone`: Número de Yape
- `PlinPhone`: Número de Plin
- `QRPhysicalAvailable`: Si tienen QR físico (boolean)
- `MobileAvailable`: Si tienen celular del negocio (boolean)

## 🎨 UI Simplificada

Como el POS es fijo, el panel puede ser más simple:
- No necesita ser modal grande
- Puede ser un panel lateral o parte del flujo normal
- Botón "Cobrar" siempre visible
- Información clara y concisa

## ✅ Ventajas de este Enfoque

1. **Flexible**: Funciona con o sin celular
2. **Compatible**: Funciona con QR físico existente
3. **Simple**: No requiere girar pantalla
4. **Rápido**: Flujo directo, sin pasos extra
5. **Robusto**: Múltiples opciones de pago

