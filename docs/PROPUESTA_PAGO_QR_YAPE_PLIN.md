# Propuesta: Sistema de Pago QR para Yape/Plin

## 📋 Resumen
Implementar un sistema de generación y visualización de códigos QR para pagos con Yape/Plin en el POS, permitiendo que el cajero muestre el QR al cliente para que escanee y complete el pago.

## 🎯 Objetivos
1. Generar códigos QR dinámicos con información de pago
2. Mostrar el QR en el POS cuando se selecciona Yape/Plin
3. Permitir confirmación manual del pago por parte del cajero
4. Opcional: Verificación automática de pagos (futuro)

## 💡 Opciones de Implementación

### **Opción 1: QR Estático con Información de Pago (Recomendada - Fase 1)**

#### Descripción
Generar un QR que contenga la información necesaria para que el cliente realice el pago manualmente desde su app Yape/Plin.

#### Información del QR:
```
Formato: Texto plano o JSON
Contenido:
- Número de teléfono/cuenta: 999888777 (configurable por tienda)
- Monto: S/ 150.00
- Concepto: "Venta #B001-000123"
- Referencia: "B001-000123"
```

#### Flujo:
1. Cliente selecciona productos y va a pagar
2. Cajero selecciona "Yape/Plin" como método de pago
3. Se muestra un modal/pantalla con:
   - QR code grande y visible
   - Número de teléfono/cuenta
   - Monto a pagar
   - Número de referencia/venta
   - Botón "Pago Confirmado" para el cajero
4. Cliente escanea el QR con su app Yape/Plin
5. Cliente completa el pago en su app
6. Cajero verifica visualmente o pregunta al cliente
7. Cajero presiona "Pago Confirmado"
8. Se procesa la venta

#### Ventajas:
- ✅ Simple de implementar
- ✅ No requiere integración con APIs externas
- ✅ Funciona inmediatamente
- ✅ Bajo costo

#### Desventajas:
- ❌ Requiere confirmación manual
- ❌ Posible error humano

---

### **Opción 2: QR con Link de Pago (Fase 2 - Avanzado)**

#### Descripción
Generar un QR que redirija a una página web donde el cliente puede completar el pago directamente.

#### Flujo:
1. Se genera un link único de pago: `https://minimarketcamucha.com/pagar/abc123xyz`
2. El QR contiene este link
3. Cliente escanea y es redirigido a página de pago
4. Cliente ingresa su número de Yape/Plin y confirma
5. Sistema verifica el pago automáticamente
6. Se procesa la venta automáticamente

#### Ventajas:
- ✅ Verificación automática
- ✅ Mejor experiencia de usuario
- ✅ Menos errores

#### Desventajas:
- ❌ Requiere desarrollo de página de pago
- ❌ Requiere integración con APIs de Yape/Plin (si disponible)
- ❌ Más complejo

---

### **Opción 3: QR Dinámico con Verificación en Tiempo Real (Fase 3 - Futuro)**

#### Descripción
Sistema completo con verificación automática de pagos mediante webhooks o polling.

#### Flujo:
1. Se genera QR con referencia única
2. Sistema monitorea pagos entrantes
3. Cuando detecta pago con la referencia, confirma automáticamente
4. Se procesa la venta sin intervención del cajero

#### Ventajas:
- ✅ Completamente automático
- ✅ Sin errores humanos
- ✅ Mejor experiencia

#### Desventajas:
- ❌ Requiere integración con APIs de Yape/Plin
- ❌ Requiere servidor webhook
- ❌ Más costoso y complejo

---

## 🛠️ Implementación Recomendada: Opción 1

### Componentes Necesarios:

#### 1. **Configuración de Cuentas de Pago**
- Guardar número de teléfono/cuenta de Yape
- Guardar número de teléfono/cuenta de Plin
- Configurable desde panel de administración

#### 2. **Componente QR Generator**
- Librería: `qrcode` o `angularx-qrcode`
- Generar QR con información de pago
- Mostrar en modal grande y visible

#### 3. **Modal de Pago QR**
- Mostrar QR code
- Mostrar información de pago (monto, referencia, número)
- Botón "Pago Confirmado"
- Botón "Cancelar"
- Timer opcional (ej: 5 minutos)

#### 4. **Flujo de Venta Modificado**
- Cuando se selecciona Yape/Plin:
  - No procesar venta inmediatamente
  - Mostrar modal con QR
  - Esperar confirmación del cajero
  - Luego procesar venta

### Estructura de Datos:

```typescript
interface PaymentQRData {
  phoneNumber: string;      // Número de Yape/Plin
  amount: number;           // Monto a pagar
  reference: string;         // Referencia de venta (ej: "B001-000123")
  concept: string;          // Concepto del pago
  paymentType: 'Yape' | 'Plin';
  saleId?: string;         // ID temporal de la venta
}
```

### Formato del QR:

**Opción A: Texto Simple (Recomendado)**
```
YAPE:999888777
MONTO:150.00
REF:V001-000123
```

**Opción B: JSON**
```json
{
  "type": "yape",
  "phone": "999888777",
  "amount": 150.00,
  "reference": "V001-000123",
  "concept": "Venta Minimarket Camucha"
}
```

**Opción C: Link (si implementamos Opción 2)**
```
https://minimarketcamucha.com/pagar/V001-000123
```

---

## 📱 Diseño de UI/UX

### Modal de Pago QR:

```
┌─────────────────────────────────────┐
│  Pago con Yape/Plin                 │
│  ─────────────────────────────────  │
│                                      │
│      ┌──────────────┐                │
│      │              │                │
│      │   [QR CODE]  │                │
│      │              │                │
│      └──────────────┘                │
│                                      │
│  Monto: S/ 150.00                    │
│  Referencia: V001-000123             │
│  Teléfono: 999 888 777               │
│                                      │
│  [ ] Pago Confirmado                 │
│                                      │
│  [Cancelar]  [Confirmar Pago]        │
└─────────────────────────────────────┘
```

### Características:
- QR grande (mínimo 300x300px)
- Información clara y visible
- Botones grandes y accesibles
- Opción de copiar número de teléfono
- Timer visual opcional

---

## 🔧 Pasos de Implementación

### Fase 1: Configuración Básica
1. ✅ Crear entidad/configuración para números de Yape/Plin
2. ✅ Agregar campos en base de datos
3. ✅ Crear componente de generación de QR
4. ✅ Crear modal de pago QR

### Fase 2: Integración con POS
1. ✅ Modificar flujo de `processSale()` para Yape/Plin
2. ✅ Mostrar modal antes de procesar venta
3. ✅ Esperar confirmación del cajero
4. ✅ Procesar venta después de confirmación

### Fase 3: Mejoras (Opcional)
1. ⏳ Timer de expiración
2. ⏳ Historial de pagos QR
3. ⏳ Notificaciones
4. ⏳ Integración con APIs (futuro)

---

## 📦 Dependencias Necesarias

```json
{
  "dependencies": {
    "qrcode": "^1.5.3",
    // o
    "angularx-qrcode": "^15.0.0"
  }
}
```

---

## 🎨 Consideraciones de Diseño

1. **Responsive**: El QR debe verse bien en desktop y móvil
2. **Accesibilidad**: Texto alternativo, contraste adecuado
3. **Modo Oscuro**: Compatible con tema oscuro
4. **Impresión**: Opción de imprimir QR (opcional)

---

## 🔐 Seguridad

1. **Referencias Únicas**: Cada venta tiene referencia única
2. **Validación de Monto**: Verificar que el monto no cambie
3. **Timeout**: Expirar QR después de cierto tiempo
4. **Logs**: Registrar todos los intentos de pago

---

## 📊 Métricas a Considerar

- Tiempo promedio de pago con QR
- Tasa de éxito de pagos QR
- Errores de confirmación manual
- Tiempo de expiración óptimo

---

## 🚀 Próximos Pasos

1. **Decidir formato del QR** (Texto simple recomendado)
2. **Implementar configuración de cuentas** en admin
3. **Crear componente QR Generator**
4. **Crear modal de pago QR**
5. **Integrar con flujo de venta**
6. **Probar con casos reales**

---

## 💬 Preguntas para Decidir

1. ¿Prefieren QR con texto simple o JSON?
2. ¿Quieren timer de expiración?
3. ¿Necesitan opción de imprimir QR?
4. ¿Quieren separar Yape y Plin o combinarlos?
5. ¿Prefieren modal o pantalla completa para el QR?

---

## 📝 Notas

- Esta implementación es escalable: podemos empezar con Opción 1 y evolucionar a Opción 2 o 3
- La confirmación manual es aceptable para MVP
- Podemos agregar verificación automática después si hay demanda

