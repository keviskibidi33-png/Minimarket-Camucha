# 📁 Configuración de Assets (Imágenes) para Producción

## ✅ Verificación de Configuración

### 1. Angular.json - Configuración de Assets

**Archivo:** `minimarket-web/angular.json`

**Configuración actual:**
```json
"assets": [
  "src/favicon.ico",
  "src/assets"
]
```

✅ **Correcto**: La carpeta `src/assets` se copia completa al build.

**Ubicación en el build:**
- Build output: `dist/minimarket-web/browser/assets/`
- Todos los archivos de `src/assets/` se copian a `dist/minimarket-web/browser/assets/`

---

### 2. Dockerfile - Copia del Build

**Archivo:** `minimarket-web/Dockerfile`

**Línea clave:**
```dockerfile
COPY --from=build /app/dist/minimarket-web/browser /usr/share/nginx/html
```

✅ **Correcto**: Copia todo el contenido de `browser/` (que incluye `assets/`) a `/usr/share/nginx/html`

**Ruta final en el contenedor:**
- Logo: `/usr/share/nginx/html/assets/logo.png`
- Favicon: `/usr/share/nginx/html/assets/favicon.ico`
- Otros assets: `/usr/share/nginx/html/assets/[archivo]`

---

### 3. Rutas en Templates

**Formato correcto:** `assets/logo.png` (sin barra inicial)

✅ **Ya corregido en:**
- `main-layout.component.html` (logo en sidebar)
- `store-header.component.html` (logo en header)
- `receipt-print.component.ts` (logo en boletas)
- `receipt-settings.component.ts` (logo en configuración)
- `index.html` (favicon y apple-touch-icon)

---

## 📋 Archivos de Assets Requeridos

### Imágenes Esenciales

1. **`src/assets/logo.png`** ✅
   - Logo principal del minimarket
   - Usado en: navbar, sidebar, boletas, facturas
   - **Ruta en producción:** `/usr/share/nginx/html/assets/logo.png`
   - **URL accesible:** `https://minimarket.edvio.app/assets/logo.png`

2. **`src/assets/favicon.ico`** ✅
   - Favicon del navegador
   - **Ruta en producción:** `/usr/share/nginx/html/assets/favicon.ico`

3. **`src/assets/angelqr.jpg`** ✅
   - QR de pago (Yape/Plin)
   - **Ruta en producción:** `/usr/share/nginx/html/assets/angelqr.jpg`

---

## 🔍 Verificación del Build

### Estructura del Build de Angular 17+

Con el builder `@angular-devkit/build-angular:application`, la estructura es:

```
dist/minimarket-web/
  └── browser/          ← Este es el directorio que se copia
      ├── index.html
      ├── favicon.ico
      ├── assets/       ← Assets copiados aquí
      │   ├── logo.png
      │   ├── favicon.ico
      │   └── angelqr.jpg
      └── [archivos JS/CSS compilados]
```

**Dockerfile copia:**
- Desde: `/app/dist/minimarket-web/browser`
- Hacia: `/usr/share/nginx/html`

**Resultado final:**
```
/usr/share/nginx/html/
  ├── index.html
  ├── favicon.ico
  ├── assets/
  │   ├── logo.png      ← ✅ Aquí debe estar
  │   ├── favicon.ico
  │   └── angelqr.jpg
  └── [archivos JS/CSS]
```

---

## ✅ Checklist de Verificación

- [x] `angular.json` tiene `"src/assets"` en la configuración de assets
- [x] `Dockerfile` copia desde `dist/minimarket-web/browser` correctamente
- [x] Rutas en templates usan `assets/logo.png` (sin barra inicial)
- [x] `logo.png` existe en `src/assets/logo.png`
- [x] `favicon.ico` existe en `src/assets/favicon.ico`
- [x] `angelqr.jpg` existe en `src/assets/angelqr.jpg`

---

## 🚨 Si el Logo Sigue Sin Cargar

### Verificación Local del Build

1. **Construir localmente:**
   ```bash
   cd minimarket-web
   npm run build -- --configuration production
   ```

2. **Verificar que el logo esté en el build:**
   ```bash
   ls dist/minimarket-web/browser/assets/logo.png
   ```

3. **Si falta, verificar:**
   - Que `src/assets/logo.png` existe
   - Que no está en `.gitignore`
   - Que `angular.json` tiene `"src/assets"` configurado

### Verificación en el Contenedor Docker

1. **Entrar al contenedor:**
   ```bash
   docker exec -it minimarket-web ls -la /usr/share/nginx/html/assets/
   ```

2. **Verificar que el logo existe:**
   ```bash
   docker exec -it minimarket-web ls -la /usr/share/nginx/html/assets/logo.png
   ```

3. **Si falta, el problema está en:**
   - El build de Angular (no copia assets)
   - El Dockerfile (no copia correctamente)

---

## 📝 Cómo Cambiar el Logo en el Futuro

1. **Reemplazar el archivo:**
   - Edita o reemplaza `minimarket-web/src/assets/logo.png`
   - Mantén el mismo nombre: `logo.png`
   - Mantén el mismo formato (PNG recomendado)

2. **Rebuild y redeploy:**
   ```bash
   # En Coolify, simplemente haz Redeploy del servicio 'web'
   # O localmente:
   cd minimarket-web
   npm run build -- --configuration production
   docker-compose build web
   docker-compose up -d web
   ```

3. **Verificar:**
   - Abre `https://minimarket.edvio.app/assets/logo.png` en el navegador
   - Debe mostrar el nuevo logo

---

## 🔧 Correcciones Aplicadas

### 1. Avatar de Usuario Corregido

**Archivo:** `minimarket-web/src/app/layout/main-layout/main-layout.component.html`

**Antes:** Usaba `assets/logo.png` como avatar de usuario (incorrecto)

**Ahora:** Usa iniciales del usuario o icono de persona (correcto)

```html
@if (authService.currentUser()?.firstName || authService.currentUser()?.lastName) {
  <div class="flex items-center justify-center w-10 h-10 rounded-full bg-primary text-white text-sm font-bold">
    {{ (authService.currentUser()!.firstName?.charAt(0) || authService.currentUser()!.lastName?.charAt(0) || 'U').toUpperCase() }}
  </div>
} @else {
  <div class="flex items-center justify-center w-10 h-10 rounded-full bg-gray-300 dark:bg-gray-600">
    <span class="material-symbols-outlined text-gray-600 dark:text-gray-300">person</span>
  </div>
}
```

---

## ✅ Estado Final

- ✅ `angular.json` configurado correctamente
- ✅ `Dockerfile` copia el build correcto
- ✅ Rutas en templates corregidas (sin barra inicial)
- ✅ Avatar de usuario corregido (no usa logo)
- ✅ Logo debe estar en `src/assets/logo.png` y agregado a git

**Ruta final del logo en producción:**
```
/usr/share/nginx/html/assets/logo.png
```

**URL accesible:**
```
https://minimarket.edvio.app/assets/logo.png
```

