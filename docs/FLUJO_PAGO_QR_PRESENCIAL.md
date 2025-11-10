# Flujo de Pago QR Presencial - Yape/Plin

## 🏪 Flujo desde el POS (Cajero)

### Paso 1: Cliente selecciona productos
- Cliente agrega productos al carrito
- Cajero ve los productos en el POS

### Paso 2: Cliente decide pagar
- Cliente indica que quiere pagar
- Cajero revisa el total en el POS

### Paso 3: Selección de método de pago
- Cajero selecciona "Yape/Plin" en el dropdown de métodos de pago
- **Acción automática**: Se muestra un modal grande con el QR code

### Paso 4: Mostrar QR al cliente
- Modal aparece en pantalla con:
  - QR code grande y visible
  - Número de teléfono de la cuenta
  - Monto a pagar: S/ 150.00
  - Referencia de venta: V001-000123
  - Botones: "Cancelar" y "Pago Confirmado" (deshabilitado inicialmente)

### Paso 5: Cliente escanea y paga
- Cliente abre su app Yape/Plin
- Cliente escanea el QR code
- La app de Yape/Plin muestra:
  - Nombre del negocio
  - Monto a pagar
  - Número de referencia
- Cliente confirma el pago en su app
- Cliente muestra confirmación al cajero (pantalla del celular)

### Paso 6: Cajero verifica y confirma
- Cajero verifica que el cliente pagó (ve la confirmación en el celular del cliente)
- Cajero presiona botón "Pago Confirmado" en el POS
- **Acción**: Se procesa la venta automáticamente
- Se genera la boleta/factura
- Se actualiza el stock
- Se muestra mensaje de éxito

### Paso 7: Finalización
- Se puede enviar boleta por email (opcional)
- Se puede imprimir boleta
- Carrito se limpia automáticamente
- Modal se cierra

---

## 👤 Flujo desde el Cliente (Usuario)

### Paso 1: Selección de productos
- Cliente selecciona productos físicamente
- Cliente lleva productos al mostrador

### Paso 2: Revisión de compra
- Cajero muestra total en pantalla
- Cliente confirma productos y total

### Paso 3: Decisión de pago
- Cliente decide pagar con Yape/Plin
- Cliente informa al cajero

### Paso 4: Escaneo del QR
- Cajero muestra QR en pantalla del POS
- Cliente abre app Yape/Plin en su celular
- Cliente escanea el QR code mostrado en la pantalla

### Paso 5: Confirmación en la app
- App Yape/Plin muestra:
  - Nombre del negocio: "Minimarket Camucha"
  - Monto: S/ 150.00
  - Referencia: V001-000123
- Cliente revisa la información
- Cliente ingresa su PIN/confirmación
- Cliente presiona "Pagar" en su app

### Paso 6: Mostrar confirmación
- App muestra "Pago realizado exitosamente"
- Cliente muestra la pantalla de confirmación al cajero
- Cajero verifica y confirma en el POS

### Paso 7: Recibir comprobante
- Cliente recibe boleta/factura (impresa o por email)
- Cliente se lleva sus productos

---

## 🔄 Diagrama de Flujo Completo

```
[CLIENTE]                    [CAJERO/POS]                    [SISTEMA]
    |                              |                              |
    |-- Selecciona productos -->   |                              |
    |                              |-- Agrega al carrito -->      |
    |                              |                              |
    |<-- Ve total en pantalla --   |                              |
    |                              |                              |
    |-- "Pago con Yape" -->        |                              |
    |                              |-- Selecciona Yape/Plin -->  |
    |                              |                              |
    |                              |-- Genera QR -->              |
    |                              |-- Muestra modal QR -->       |
    |                              |                              |
    |<-- Ve QR en pantalla ------  |                              |
    |                              |                              |
    |-- Abre app Yape/Plin -->     |                              |
    |-- Escanea QR -->             |                              |
    |                              |                              |
    |-- Confirma pago en app -->   |                              |
    |                              |                              |
    |-- Muestra confirmación -->   |                              |
    |                              |-- Verifica pago -->          |
    |                              |-- Presiona "Confirmado" -->  |
    |                              |                              |
    |                              |                              |-- Procesa venta -->
    |                              |                              |-- Actualiza stock -->
    |                              |                              |-- Genera boleta -->
    |                              |                              |
    |<-- Recibe boleta ----------- |<-- Muestra éxito ----------- |
    |                              |                              |
    |-- Se lleva productos -->     |                              |
```

---

## 📱 Detalles Técnicos del Flujo

### Cuando se selecciona Yape/Plin:

1. **Validación**:
   - Verificar que hay productos en el carrito
   - Verificar que el total > 0
   - Verificar que hay número de Yape/Plin configurado

2. **Generación de QR**:
   - Crear referencia única: `V001-000123` (formato: TipoDoc-Secuencial)
   - Generar QR con formato:
     ```
     YAPE:999888777
     MONTO:150.00
     REF:V001-000123
     ```
   - Mostrar modal con QR

3. **Estado de la venta**:
   - La venta NO se procesa inmediatamente
   - Se crea un "borrador" temporal
   - Se espera confirmación del cajero

4. **Confirmación**:
   - Cajero presiona "Pago Confirmado"
   - Se procesa la venta real
   - Se actualiza stock
   - Se genera boleta/factura
   - Se limpia el carrito

5. **Cancelación**:
   - Si el cliente no paga o cancela
   - Cajero presiona "Cancelar"
   - Se cierra el modal
   - El carrito permanece intacto
   - Se puede cambiar método de pago

---

## 🎯 Casos Especiales

### Caso 1: Cliente no tiene app instalada
- **Solución**: Mostrar número de teléfono grande
- Cliente puede pagar manualmente ingresando el número
- Cajero verifica igual

### Caso 2: QR no se escanea bien
- **Solución**: Botón "Mostrar número" para pago manual
- Cliente ingresa número manualmente en su app

### Caso 3: Cliente paga monto incorrecto
- **Solución**: Cajero puede cancelar y volver a generar QR
- O ajustar manualmente si la diferencia es pequeña

### Caso 4: Cliente paga pero cajero no confirma
- **Solución**: Timer opcional (ej: 5 minutos)
- Si expira, se cancela automáticamente
- Se puede volver a generar QR

### Caso 5: Múltiples clientes esperando
- **Solución**: Modal grande y visible
- QR se puede ver desde lejos
- Proceso rápido (30-60 segundos)

---

## ⚙️ Configuración Necesaria

### En el Panel de Administración:
- **Número de Yape**: 999 888 777
- **Número de Plin**: 999 888 666
- **Nombre del negocio**: "Minimarket Camucha"
- **QR por defecto**: Yape o Plin

### En el POS:
- Seleccionar método: "Yape/Plin"
- O separar: "Yape" y "Plin" (dos opciones)

---

## 🔒 Seguridad y Validaciones

1. **Referencia única**: Cada venta tiene referencia única
2. **Monto fijo**: El monto no puede cambiar después de generar QR
3. **Timeout**: QR expira después de X minutos (configurable)
4. **Logs**: Registrar todos los intentos de pago QR
5. **Validación**: Verificar que el monto pagado = monto del QR

---

## 📊 Métricas a Monitorear

- Tiempo promedio de pago con QR
- Tasa de éxito de pagos QR
- Tiempo de escaneo promedio
- Errores de confirmación
- Cancelaciones

---

## 🚀 Próximos Pasos de Implementación

1. ✅ Crear configuración de números Yape/Plin
2. ✅ Crear componente generador de QR
3. ✅ Crear modal de pago QR
4. ✅ Modificar flujo de `processSale()` para Yape/Plin
5. ✅ Agregar validaciones
6. ✅ Probar flujo completo

