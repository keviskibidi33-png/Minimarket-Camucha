# Implementación Simple de Pago QR - Usando QR Existente

## 📋 Resumen
Usar el QR físico/imagen que ya tiene el negocio. No necesitamos generar QR dinámicamente, solo mostrarlo junto con la información de pago.

## 🎯 Flujo Simplificado

### Cuando se selecciona Yape/Plin:

1. **POS muestra panel con**:
   - Imagen del QR (que ya tienen)
   - Número de teléfono (Yape o Plin)
   - Monto a pagar
   - Referencia de venta
   - Botón "Copiar Teléfono"
   - Botón "Cobrar"

2. **Opciones para el cajero**:
   - Mostrar QR del celular del negocio (si lo tienen)
   - O indicar al cliente que use el QR físico de la repisa
   - O copiar número para pago manual

3. **Cliente paga**:
   - Escanea QR (del celular o físico)
   - O paga manualmente con el número

4. **Cajero valida y cobra**:
   - Verifica que el cliente pagó
   - Presiona "Cobrar"
   - Se procesa la venta

## 🗂️ Almacenamiento del QR

### Opción 1: URL de Imagen (Recomendada)
- Subir QR a servidor/almacenamiento
- Guardar URL en BrandSettings
- Mostrar imagen desde URL

### Opción 2: Base64
- Convertir QR a Base64
- Guardar en BrandSettings
- Mostrar imagen desde Base64

### Opción 3: Archivo Local
- Guardar QR en carpeta de assets
- Referenciar desde código
- Más simple pero menos flexible

## 📝 Campos Necesarios en BrandSettings

```csharp
public class BrandSettings {
    // ... campos existentes ...
    public string? YapePhone { get; set; }
    public string? PlinPhone { get; set; }
    public string? YapeQRUrl { get; set; }  // URL de la imagen del QR
    public string? PlinQRUrl { get; set; }  // URL de la imagen del QR
}
```

## 🎨 Componente Simple

```typescript
// QRPaymentPanel Component
- Muestra imagen del QR (desde URL)
- Muestra número de teléfono
- Muestra monto y referencia
- Botón "Copiar Teléfono"
- Botón "Cobrar"
```

## ✅ Ventajas

1. **Simple**: No necesita librerías de generación
2. **Rápido**: Solo mostrar imagen existente
3. **Flexible**: Funciona con QR físico o digital
4. **Económico**: Sin dependencias extra

## 📦 Pasos de Implementación

1. ✅ Agregar campos YapePhone, PlinPhone, YapeQRUrl, PlinQRUrl a BrandSettings
2. ✅ Crear migración
3. ✅ Actualizar DTOs
4. ✅ Crear componente QRPaymentPanel (simple, solo muestra imagen)
5. ✅ Integrar con flujo de venta
6. ✅ Agregar opción de subir QR en panel de admin

