# Assets - Minimarket Camucha

Esta carpeta contiene los archivos estáticos (imágenes, iconos, logos) del proyecto.

## Ubicación de archivos

Coloca tus archivos aquí:

### Icono (Favicon)
- **Archivo:** `favicon.ico`
- **Ubicación:** `minimarket-web/src/favicon.ico` (en la raíz de `src/`, no en assets)
- **Uso:** Se muestra en la pestaña del navegador

### Logo
- **Archivo:** `logo.png` o `logo.svg` (recomendado: PNG para compatibilidad)
- **Ubicación:** `minimarket-web/src/assets/logo.png`
- **Uso:** Se puede usar en componentes con la ruta `/assets/logo.png`

### Otros assets
- **Imágenes:** Colócalas en esta carpeta (`src/assets/`)
- **Ejemplo:** `src/assets/images/productos/`, `src/assets/images/categorias/`, etc.

## Cómo usar los assets en el código

```typescript
// En componentes TypeScript
imageUrl = '/assets/logo.png';

// En templates HTML
<img src="/assets/logo.png" alt="Logo Camucha">

// En CSS
background-image: url('/assets/logo.png');
```

## Nota importante

Los archivos en esta carpeta se copian automáticamente al directorio `dist/` cuando se compila el proyecto, por lo que las rutas siempre deben comenzar con `/assets/`.

