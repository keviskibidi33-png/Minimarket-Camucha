# Pruebas de Validación - Admin Setup Wizard

## ✅ Checklist de Funcionalidad

### Paso 1: Información Básica
- [x] Campo `storeName` - Requerido, máximo 200 caracteres
- [x] Campo `businessType` - Requerido, dropdown con opciones
- [x] Campo `phone` - Requerido, máximo 20 caracteres
- [x] Campo `description` - Opcional, máximo 1000 caracteres
- [x] Campo `whatSells` - Opcional, máximo 500 caracteres
- [x] Checkbox `isVirtual` - Si está marcado, no requiere dirección física
- [x] Campos `sedeAddress`, `sedeCity`, `sedeRegion` - Requeridos solo si NO es virtual
- [x] Validación: Si es virtual, no requiere dirección/ciudad

### Paso 2: Branding y Diseño
- [x] Campo `logoFile` - Opcional, archivo de imagen
- [x] Campo `faviconFile` - Opcional, archivo de imagen
- [x] Campo `primaryColor` - Requerido, color picker + input hex
- [x] Campo `secondaryColor` - Requerido, color picker + input hex
- [x] Campo `email` - Opcional, validación de email
- [x] Campo `ruc` - Opcional, máximo 20 caracteres
- [x] Campo `slogan` - Opcional, máximo 500 caracteres

### Paso 3: Configuración del Sistema
- [x] Categorías predefinidas - Checkboxes, al menos una debe estar seleccionada
- [x] Crear categorías personalizadas - Botón "Nueva Categoría", formulario inline
- [x] Validación: Al menos una categoría (predefinida o personalizada) debe estar seleccionada
- [x] Campo `systemUsers` - Requerido, dropdown (1-5, 6-10, 11-20, 21+)
- [x] **Personalización de Página:**
  - [x] Campo `homeTitle` - Opcional, máximo 200 caracteres
  - [x] Campo `homeSubtitle` - Opcional, máximo 500 caracteres
  - [x] Campo `homeDescription` - Opcional, máximo 1000 caracteres
  - [x] Campo `homeBannerImage` - Opcional, archivo de imagen con preview

### Paso 4: Información de Pago y Envío
- [x] **Yape/Plin Unificado:**
  - [x] Campo `yapePlinPhone` - Opcional, máximo 20 caracteres
  - [x] Campo `yapePlinQRFile` - Opcional, archivo de imagen QR
- [x] **Cuenta Bancaria (Opcional):**
  - [x] Campo `bankName` - Opcional, máximo 100 caracteres
  - [x] Campo `bankAccountType` - Opcional, dropdown (Ahorros/Corriente)
  - [x] Campo `bankAccountNumber` - Opcional, máximo 50 caracteres
  - [x] Campo `bankCCI` - Opcional, máximo 50 caracteres
- [x] **Opciones de Entrega:**
  - [x] Campo `deliveryType` - Requerido, dropdown
  - [x] Si es virtual: Solo muestra "Solo envío a domicilio"
  - [x] Si NO es virtual: Muestra todas las opciones (Ambos, Solo recogida, Solo envío)
  - [x] Campo `deliveryCost` - Opcional, número decimal
  - [x] Campo `deliveryZones` - Opcional, texto libre

### Paso 5: Crear Usuarios
- [x] Checkbox `createCashier` - Si está marcado, requiere datos del cajero
- [x] Campo `cashierEmail` - Requerido si createCashier=true, validación de email
- [x] Campo `cashierPassword` - Requerido si createCashier=true, mínimo 6 caracteres
- [x] Campo `cashierFirstName` - Requerido si createCashier=true
- [x] Campo `cashierLastName` - Requerido si createCashier=true
- [x] Campo `cashierDni` - Requerido si createCashier=true, patrón 8 dígitos

## ✅ Integración Backend

### Endpoint: POST /api/auth/admin-setup
- [x] Acepta `[FromForm] IFormCollection`
- [x] Requiere autenticación y rol "Administrador"
- [x] Procesa archivos: logoFile, faviconFile, yapeQRFile, plinQRFile, homeBannerImage
- [x] Guarda todos los campos en `BrandSettings`
- [x] Crea categorías si no existen
- [x] Crea Sede si no es virtual
- [x] Crea usuario cajero si se solicita

### Base de Datos
- [x] Migración aplicada: `AddHomePageCustomizationToBrandSettings`
- [x] Campos en `BrandSettings`: HomeTitle, HomeSubtitle, HomeDescription, HomeBannerImageUrl
- [x] Todos los campos son opcionales (nullable)

## ✅ Integración Frontend - Página Principal

### HomeComponent
- [x] Carga `BrandSettings` al inicializar
- [x] Usa `homeBannerImageUrl` para el banner (si existe)
- [x] Usa `homeTitle` para el título (si existe, sino usa storeName)
- [x] Usa `homeSubtitle` para el subtítulo (si existe)
- [x] Usa `homeDescription` para la descripción (si existe)
- [x] Valores por defecto si no hay datos personalizados

### BrandSettingsService
- [x] Interfaz `BrandSettings` incluye campos de personalización
- [x] Interfaz `UpdateBrandSettings` incluye campos de personalización
- [x] No carga automáticamente en constructor (evita NG0203)

## ✅ Validaciones y Errores

- [x] Error NG0203 corregido en `settings.component.ts` (effect dentro de afterNextRender)
- [x] Error NG0203 corregido en `brand-settings.service.ts` (no carga en constructor)
- [x] Validación de campos requeridos por paso
- [x] Validación condicional según tipo de negocio (virtual/físico)
- [x] Manejo de errores en el formulario

## 🧪 Pruebas Manuales Recomendadas

1. **Flujo Completo del Wizard:**
   - Completar todos los pasos del 1 al 5
   - Verificar que los datos se guardan correctamente
   - Verificar que se redirige a `/admin` después de completar

2. **Personalización de Página:**
   - Llenar campos de personalización en Paso 3
   - Verificar que se guardan en la base de datos
   - Verificar que se muestran en la página principal (`/`)

3. **Validaciones:**
   - Intentar avanzar sin completar campos requeridos
   - Verificar que muestra mensajes de error apropiados
   - Verificar validación condicional (virtual vs físico)

4. **Categorías Personalizadas:**
   - Crear una categoría personalizada
   - Verificar que se agrega a la lista
   - Verificar que se puede eliminar
   - Verificar que se envía al backend

5. **Opciones de Entrega:**
   - Marcar "Mi negocio es virtual" y verificar que solo muestra "Solo envío"
   - Desmarcar y verificar que muestra todas las opciones

