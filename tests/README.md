# Pruebas E2E del Flujo de Usuario

Este directorio contiene pruebas automatizadas end-to-end usando Playwright para validar el flujo completo de usuarios en Minimarket Camucha.

## Instalación

```bash
npm install -D @playwright/test
npx playwright install
```

## Ejecutar Pruebas

### Ejecutar todas las pruebas
```bash
npx playwright test
```

### Ejecutar una prueba específica
```bash
npx playwright test user-flow-test
```

### Ejecutar en modo UI (interactivo)
```bash
npx playwright test --ui
```

### Ver reporte HTML
```bash
npx playwright show-report
```

## Pruebas Incluidas

### 1. Flujo Completo de Usuario
- Registro de nuevo usuario
- Completar perfil con dirección
- Logout y Login
- Navegar por la tienda
- Agregar productos al carrito
- Verificar perfil y direcciones guardadas

### 2. Validación de DNI Duplicado
- Verifica que no se puede registrar con un DNI ya existente

### 3. Validación de Formulario de Completar Perfil
- Verifica campos requeridos
- Verifica que nombre y teléfono son readonly (pre-llenados)

## Requisitos Previos

1. **Backend corriendo**: El servidor backend debe estar ejecutándose en `http://localhost:5000`
2. **Frontend corriendo**: El servidor frontend debe estar ejecutándose en `http://localhost:4200`
3. **Base de datos**: La base de datos debe estar configurada y accesible

## Notas

- Las pruebas crean usuarios de prueba con emails únicos usando timestamps
- Los datos de prueba se definen en `user-flow-test.ts`
- Las pruebas limpian automáticamente después de ejecutarse (si se implementa cleanup)

