# Cypress E2E Testing - Minimarket Camucha

Este directorio contiene todos los tests End-to-End (E2E) para el frontend Angular del sistema Minimarket.

## Estructura

```
cypress/
├── e2e/              # Tests E2E organizados por módulo
│   ├── auth/        # Tests de autenticación
│   ├── admin/       # Tests del panel de administración
│   ├── pos/         # Tests del punto de venta
│   ├── sales/       # Tests de ventas
│   ├── inventory/   # Tests de inventario
│   ├── reports/     # Tests de reportes
│   └── responsive/  # Tests responsive
├── fixtures/        # Datos de prueba (JSON)
├── support/         # Custom commands y page objects
│   └── page-objects/
└── cypress.config.ts
```

## Comandos Disponibles

### Ejecutar tests en modo interactivo
```bash
npm run cy:open
```

### Ejecutar todos los tests en modo headless
```bash
npm run cy:run
```

### Ejecutar tests en Chrome
```bash
npm run cy:run:chrome
```

### Ejecutar tests en Firefox
```bash
npm run cy:run:firefox
```

### Ejecutar tests en modo móvil
```bash
npm run cy:run:mobile
```

### Ejecutar tests con servidor automático
```bash
npm run test:e2e
```

## Configuración

Los tests están configurados para ejecutarse contra:
- **Frontend**: `http://localhost:4200`
- **Backend API**: `http://localhost:5000/api`

Asegúrate de que ambos servidores estén corriendo antes de ejecutar los tests.

## Credenciales de Prueba

Las credenciales de prueba están configuradas en `cypress.config.ts`:

- **Admin**: `admin` / `Admin@1234`
- **Cajero**: `cajero` / `Cajero@1234`

## Custom Commands

Se han creado comandos personalizados para facilitar los tests:

- `cy.login(email, password)` - Iniciar sesión
- `cy.loginAsAdmin()` - Iniciar sesión como administrador
- `cy.loginAsCajero()` - Iniciar sesión como cajero
- `cy.logout()` - Cerrar sesión
- `cy.createProduct(product)` - Crear un producto
- `cy.searchProduct(query)` - Buscar producto
- `cy.addProductToCart(productName, quantity)` - Agregar producto al carrito
- `cy.completeSale(paymentMethod, amountPaid)` - Completar una venta

## Page Objects

Los Page Objects están en `cypress/support/page-objects/`:

- `LoginPage` - Página de login
- `POSPage` - Página del punto de venta
- `ProductsPage` - Página de gestión de productos
- `SalesPage` - Página de ventas
- `DashboardPage` - Dashboard

## Fixtures

Los datos de prueba están en `cypress/fixtures/`:

- `users.json` - Usuarios de prueba
- `products.json` - Productos de prueba
- `sales.json` - Ventas de prueba

## Notas Importantes

1. Los tests requieren que el backend esté corriendo con datos seed
2. Algunos tests pueden necesitar datos específicos en la base de datos
3. Los atributos `data-cy` deben estar presentes en los componentes para que los tests funcionen
4. Los tests se ejecutan con retries automáticos (2 intentos en modo run)

## Ejecución Continua

Para ejecutar tests en CI/CD, usar:
```bash
npm run cy:run
```

