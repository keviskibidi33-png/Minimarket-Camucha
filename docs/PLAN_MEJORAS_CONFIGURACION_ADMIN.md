# Plan de Mejoras - Configuración del Admin

## ✅ Completado

1. **Validación del Paso 1 corregida**:
   - Si es virtual: NO requiere dirección/ciudad ✅
   - Si NO es virtual: SÍ requiere dirección Y ciudad ✅
   - Validación consistente y correcta

2. **Teléfono movido al Paso 1**:
   - Campo requerido
   - Solo se pide una vez
   - Ubicado después de nombre y rubro
   - Con mensaje explicativo

## 📋 Pendiente - Mejoras Necesarias

### 1. **Información Básica Adicional (Nuevo Paso o Extensión del Paso 1)**

Agregar al wizard inicial o crear sección en "Configuración de Sistema":

#### Campos a Agregar:
- **QR de Yape**: Subir imagen del QR
- **QR de Plin**: Subir imagen del QR (opcional)
- **Número de Yape**: 999 888 777
- **Número de Plin**: 999 888 666 (opcional)
- **Cuenta Bancaria**: 
  - Banco
  - Tipo de cuenta (Ahorros/Corriente)
  - Número de cuenta
  - CCI (opcional)

#### Opciones de Envío:
- **Tipo de entrega**:
  - ☐ Solo recogida en tienda
  - ☐ Solo envíos a domicilio
  - ☐ Ambos (recogida y envío)
- **Costo de envío**: 
  - Gratis
  - Fijo (S/ X.XX)
  - Por distancia
- **Zonas de envío**: (si aplica)
  - Distritos/Localidades

### 2. **Configuración de Sistema - Personalización de Página**

Crear nueva sección en `/admin/configuraciones` con pestañas:

#### Pestaña 1: "Apariencia de la Página"
- **Textos del Home**:
  - Título principal
  - Subtítulo
  - Descripción
  - Texto del botón CTA
- **Imágenes**:
  - Banner principal
  - Imágenes de secciones
  - Galería de productos destacados
- **Layout**:
  - Estilo de tarjetas de productos
  - Orden de secciones
  - Colores adicionales

#### Pestaña 2: "Categorías"
- Lista de categorías existentes
- Crear nueva categoría
- Editar categoría
- Eliminar categoría
- Ordenar categorías
- Icono por categoría

#### Pestaña 3: "Información Básica"
- QR Yape/Plin (subir/editar)
- Cuentas bancarias
- Opciones de envío
- Horarios de atención
- Redes sociales

### 3. **Integración con Carrito de Compras**

Cuando se configura el tipo de entrega, afectar las opciones del carrito:

#### Si "Solo recogida en tienda":
- No mostrar opción de envío
- Mostrar dirección de la tienda
- Mostrar horarios de atención

#### Si "Solo envíos":
- Mostrar formulario de dirección de envío
- Calcular costo de envío
- Validar zona de envío

#### Si "Ambos":
- Mostrar selector: "Recoger en tienda" / "Envío a domicilio"
- Si recoger: mostrar dirección y horarios
- Si envío: mostrar formulario y calcular costo

## 🗂️ Estructura Propuesta

### Backend - Nuevas Entidades/Campos

#### BrandSettings (extender):
```csharp
public string? YapePhone { get; set; }
public string? PlinPhone { get; set; }
public string? YapeQRUrl { get; set; }
public string? PlinQRUrl { get; set; }
public string? BankName { get; set; }
public string? BankAccountType { get; set; } // "Ahorros" | "Corriente"
public string? BankAccountNumber { get; set; }
public string? BankCCI { get; set; }
public string DeliveryType { get; set; } = "Ambos"; // "SoloRecogida" | "SoloEnvio" | "Ambos"
public decimal? DeliveryCost { get; set; }
public string? DeliveryZones { get; set; } // JSON array
```

#### Nueva Entidad: PageSettings
```csharp
public class PageSettings {
    public string HomeTitle { get; set; }
    public string HomeSubtitle { get; set; }
    public string HomeDescription { get; set; }
    public string CTAText { get; set; }
    public string BannerImageUrl { get; set; }
    // ... más campos
}
```

### Frontend - Nuevas Secciones

#### `/admin/configuraciones/informacion-basica`
- Formulario con:
  - QR Yape/Plin
  - Cuentas bancarias
  - Opciones de envío

#### `/admin/configuraciones/pagina`
- Editor de textos del home
- Gestor de imágenes
- Configuración de layout

#### `/admin/configuraciones/categorias`
- CRUD de categorías
- Ordenamiento
- Iconos

## 🔄 Flujo de Implementación

### Fase 1: Información Básica (QR, Bancos, Envío)
1. Agregar campos a BrandSettings
2. Crear migración
3. Actualizar DTOs y handlers
4. Crear componente de configuración
5. Integrar con carrito

### Fase 2: Personalización de Página
1. Crear entidad PageSettings
2. Crear componentes de edición
3. Integrar con home page

### Fase 3: Gestión de Categorías
1. Mejorar CRUD de categorías
2. Agregar iconos
3. Agregar ordenamiento

## 📝 Notas

- La personalización actual (nombre, slogan, colores) funciona correctamente
- Necesitamos extender con más opciones
- Todo debe ser configurable desde el panel de admin
- Los cambios deben reflejarse inmediatamente en la tienda

