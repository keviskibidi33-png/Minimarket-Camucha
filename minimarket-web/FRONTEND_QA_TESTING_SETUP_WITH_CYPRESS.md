# Frontend QA Testing Setup with Cypress - Documentación Completa

## 📋 Resumen del Proyecto

Implementación completa de un sistema de testing E2E (End-to-End) con Cypress para el frontend Angular del sistema Minimarket Camucha. El sistema incluye configuración, custom commands, page objects, fixtures y tests para todos los módulos principales.

---

## 🎯 Objetivo

Crear y ejecutar pruebas E2E exhaustivas usando Cypress para garantizar que el frontend Angular funcione correctamente, sea usable y proporcione una excelente experiencia de usuario.

---

## 📁 Estructura de Archivos Creados

### Configuración Principal
```
minimarket-web/
├── cypress.config.ts                    # Configuración principal de Cypress
├── cypress/
│   ├── e2e/                             # Tests E2E organizados por módulo
│   │   ├── auth/
│   │   │   ├── login.cy.ts             # Tests de login
│   │   │   └── logout.cy.ts            # Tests de logout
│   │   ├── admin/
│   │   │   └── products.cy.ts          # Tests de gestión de productos
│   │   ├── pos/
│   │   │   └── sale-creation.cy.ts     # Tests del punto de venta
│   │   ├── sales/
│   │   │   └── sales-list.cy.ts        # Tests de listado de ventas
│   │   ├── reports/
│   │   │   └── dashboard.cy.ts         # Tests del dashboard
│   │   └── responsive/
│   │       └── mobile.cy.ts            # Tests responsive móvil
│   ├── fixtures/                        # Datos de prueba
│   │   ├── users.json                   # Usuarios de prueba
│   │   ├── products.json                # Productos de prueba
│   │   └── sales.json                   # Ventas de prueba
│   ├── support/                         # Custom commands y helpers
│   │   ├── commands.ts                  # Custom commands reutilizables
│   │   ├── helpers.ts                   # Funciones helper
│   │   ├── e2e.ts                       # Configuración de soporte
│   │   └── page-objects/                # Page Objects Pattern
│   │       ├── login.page.ts
│   │       ├── pos.page.ts
│   │       ├── products.page.ts
│   │       ├── sales.page.ts
│   │       └── dashboard.page.ts
│   └── README.md                        # Documentación de uso
```

---

## 🔧 Configuración de Cypress

### cypress.config.ts
```typescript
- baseUrl: 'http://localhost:4200'
- viewportWidth: 1920
- viewportHeight: 1080
- video: true
- screenshotOnRunFailure: true
- defaultCommandTimeout: 10000
- retries: { runMode: 2, openMode: 0 }
- env variables para testUser y testCajero
```

### Variables de Entorno Configuradas
- `testUser.email`: admin@minimarket.com
- `testUser.password`: Admin@1234
- `testCajero.email`: cajero@minimarket.com
- `testCajero.password`: Cajero@1234
- `apiUrl`: http://localhost:5000/api

---

## 🛠️ Custom Commands Implementados

### Comandos de Autenticación
- `cy.login(email, password)` - Login genérico
- `cy.loginAsAdmin()` - Login como administrador
- `cy.loginAsCajero()` - Login como cajero
- `cy.logout()` - Cerrar sesión

### Comandos de Productos
- `cy.createProduct(product)` - Crear producto
- `cy.searchProduct(query)` - Buscar producto

### Comandos de POS
- `cy.addProductToCart(productName, quantity)` - Agregar al carrito
- `cy.completeSale(paymentMethod, amountPaid)` - Completar venta

---

## 📄 Page Objects Creados

### LoginPage
- `visit()` - Visitar página de login
- `login(email, password)` - Realizar login
- `verifyFormVisible()` - Verificar formulario visible
- `verifyErrorMessage(message)` - Verificar mensaje de error
- `togglePasswordVisibility()` - Mostrar/ocultar contraseña

### POSPage
- `visit()` - Visitar POS
- `searchProduct(query)` - Buscar producto
- `selectDocumentType(type)` - Seleccionar tipo de comprobante
- `selectPaymentMethod(method)` - Seleccionar método de pago
- `completeSale()` - Completar venta
- `verifySaleCreated()` - Verificar venta creada

### ProductsPage
- `visit()` - Visitar página de productos
- `clickNewProduct()` - Click en nuevo producto
- `fillProductForm(product)` - Llenar formulario
- `searchProduct(query)` - Buscar producto
- `editProduct(productName)` - Editar producto
- `deleteProduct(productName)` - Eliminar producto

### SalesPage
- `visit()` - Visitar página de ventas
- `filterByDateRange(startDate, endDate)` - Filtrar por fecha
- `filterByDocumentType(type)` - Filtrar por tipo
- `viewSaleDetails()` - Ver detalle de venta
- `cancelSale(reason)` - Anular venta

### DashboardPage
- `visit()` - Visitar dashboard
- `verifyKPIsVisible()` - Verificar KPIs
- `verifySalesChartVisible()` - Verificar gráfico
- `verifyTopProductsVisible()` - Verificar top productos

---

## 🧪 Tests E2E Implementados

### Auth Tests
✅ **login.cy.ts**
- Display login form
- Login successful with valid credentials
- Show error with invalid credentials
- Validate required fields
- Toggle password visibility
- Navigate to forgot password

✅ **logout.cy.ts**
- Logout successfully

### Admin Tests
✅ **products.cy.ts**
- Display products list
- Search products by name
- Navigate to new product page

### POS Tests
✅ **sale-creation.cy.ts**
- Display POS interface
- Search for products
- Select document type
- Select payment method
- Display cart when empty

### Sales Tests
✅ **sales-list.cy.ts**
- Display sales list
- Filter sales by date range

### Reports Tests
✅ **dashboard.cy.ts**
- Display main KPIs
- Display sales trend chart

### Responsive Tests
✅ **mobile.cy.ts**
- Display login form on mobile
- Allow login on mobile

---

## 🏷️ Atributos data-cy Agregados

### Login Component
- `data-cy="email-input"`
- `data-cy="password-input"`
- `data-cy="login-button"`
- `data-cy="error-message"`
- `data-cy="email-error"`
- `data-cy="password-error"`
- `data-cy="toggle-password"`
- `data-cy="forgot-password-link"`

### Main Layout Component
- `data-cy="user-menu"`
- `data-cy="logout-button"`

### POS Component
- `data-cy="product-search"`
- `data-cy="document-type"`
- `data-cy="customer-search"`
- `data-cy="selected-customer"`
- `data-cy="customer-ruc"`
- `data-cy="cart-items"`
- `data-cy="cart-item-{index}"`
- `data-cy="remove-item-{index}"`
- `data-cy="subtotal"`
- `data-cy="igv"`
- `data-cy="total"`
- `data-cy="discount-percentage"`
- `data-cy="payment-method-select"`
- `data-cy="payment-{method}"`
- `data-cy="amount-paid"`
- `data-cy="change-amount"`
- `data-cy="clear-cart"`
- `data-cy="complete-sale-button"`

### Products Component
- `data-cy="search-input"`
- `data-cy="new-product-button"`
- `data-cy="products-table"`
- `data-cy="product-row"`

### Sales Component
- `data-cy="date-from"`
- `data-cy="date-to"`
- `data-cy="search-input"`
- `data-cy="sales-table"`

### Dashboard Component
- `data-cy="total-sales"`
- `data-cy="total-profit"`
- `data-cy="transactions-count"`
- `data-cy="inventory-value"`
- `data-cy="sales-chart"`
- `data-cy="top-products"`

---

## 📦 Dependencias Instaladas

### DevDependencies
```json
{
  "cypress": "^13.6.0",
  "cypress-real-events": "^1.11.0",
  "@cypress/code-coverage": "^3.12.0",
  "@faker-js/faker": "^8.3.1",
  "cypress-file-upload": "^5.0.8",
  "start-server-and-test": "^2.0.3"
}
```

---

## 🚀 Scripts NPM Agregados

```json
{
  "cy:open": "cypress open",
  "cy:run": "cypress run",
  "cy:run:chrome": "cypress run --browser chrome",
  "cy:run:firefox": "cypress run --browser firefox",
  "cy:run:mobile": "cypress run --config viewportWidth=375,viewportHeight=667",
  "test:e2e": "start-server-and-test 'ng serve' http://localhost:4200 cy:run"
}
```

---

## 🎯 Comandos de Ejecución

### Modo Interactivo (Recomendado para desarrollo)
```bash
npm run cy:open
```
Abre Cypress Test Runner con interfaz gráfica.

### Modo Headless (CI/CD)
```bash
npm run cy:run
```
Ejecuta todos los tests en modo headless.

### Navegadores Específicos
```bash
npm run cy:run:chrome
npm run cy:run:firefox
```

### Tests Responsive
```bash
npm run cy:run:mobile
```

### Con Servidor Automático
```bash
npm run test:e2e
```
Inicia el servidor Angular automáticamente antes de ejecutar tests.

---

## 📋 Prerrequisitos

1. **Backend API** corriendo en `http://localhost:5000`
2. **Frontend Angular** corriendo en `http://localhost:4200`
3. **Base de datos** con datos seed para pruebas
4. **Node.js** 18+ instalado
5. **NPM** instalado

---

## 🔄 Flujo de Trabajo Recomendado

1. **Iniciar Backend**
   ```bash
   cd src/Minimarket.API
   dotnet run
   ```

2. **Iniciar Frontend**
   ```bash
   cd minimarket-web
   npm start
   ```

3. **Ejecutar Tests**
   ```bash
   npm run cy:open
   ```

---

## 📝 Archivos Modificados

### Componentes Angular con data-cy
1. `src/app/features/auth/login/login.component.html`
2. `src/app/layout/main-layout/main-layout.component.html`
3. `src/app/features/pos/pos.component.html`
4. `src/app/features/products/products.component.html`
5. `src/app/features/sales/sales.component.html`
6. `src/app/features/dashboard/dashboard.component.html`

### Configuración
1. `package.json` - Scripts y dependencias
2. `cypress.config.ts` - Configuración de Cypress

---

## 🚧 Tareas Pendientes / Futuras Mejoras

### Tests Adicionales Necesarios
- [ ] Tests completos de CRUD de productos (crear, editar, eliminar)
- [ ] Tests de validación de formularios de productos
- [ ] Tests de filtros y búsqueda avanzada
- [ ] Tests de paginación
- [ ] Tests de exportación a Excel
- [ ] Tests completos de flujo de venta POS (agregar productos, calcular totales, completar venta)
- [ ] Tests de métodos de pago (Efectivo, Tarjeta, Yape/Plin)
- [ ] Tests de comprobantes (Boleta y Factura)
- [ ] Tests de anulación de ventas
- [ ] Tests de reimpresión de comprobantes
- [ ] Tests de gestión de clientes (CRUD completo)
- [ ] Tests de gestión de categorías (CRUD completo)
- [ ] Tests de gestión de usuarios
- [ ] Tests de movimientos de inventario
- [ ] Tests de alertas de stock bajo
- [ ] Tests de reportes detallados
- [ ] Tests de comparativas mensuales
- [ ] Tests responsive para tablet
- [ ] Tests de accesibilidad (navegación por teclado, contraste, etc.)
- [ ] Tests de performance (tiempo de carga)

### Atributos data-cy Pendientes
- [ ] Agregar data-cy a componente product-form
- [ ] Agregar data-cy a componente category-form
- [ ] Agregar data-cy a componente customer-form
- [ ] Agregar data-cy a componente sale-detail
- [ ] Agregar data-cy a componente cancel-sale
- [ ] Agregar data-cy a todos los botones de acción en tablas
- [ ] Agregar data-cy a modales y diálogos de confirmación
- [ ] Agregar data-cy a toasts y mensajes de éxito/error

### Mejoras de Configuración
- [ ] Configurar interceptors para mockear APIs en tests específicos
- [ ] Agregar configuración de code coverage
- [ ] Configurar reportes de test (HTML, JSON)
- [ ] Agregar screenshots/videos automáticos en fallos
- [ ] Configurar tests paralelos para CI/CD

### Mejoras de Page Objects
- [ ] Agregar más métodos helper a Page Objects
- [ ] Crear Page Objects para componentes faltantes
- [ ] Agregar validaciones más robustas
- [ ] Implementar wait strategies mejoradas

### Mejoras de Fixtures
- [ ] Agregar más datos de prueba variados
- [ ] Crear fixtures para diferentes escenarios
- [ ] Agregar datos para edge cases

### Documentación
- [ ] Agregar ejemplos de uso de cada custom command
- [ ] Documentar mejores prácticas
- [ ] Agregar guía de troubleshooting
- [ ] Crear diagramas de flujo de tests

---

## 🔍 Debugging y Troubleshooting

### Problemas Comunes

1. **Tests fallan por timeout**
   - Aumentar `defaultCommandTimeout` en `cypress.config.ts`
   - Verificar que el servidor esté corriendo

2. **No encuentra elementos con data-cy**
   - Verificar que el componente tenga el atributo
   - Verificar que el componente esté renderizado
   - Usar `cy.wait()` para esperar carga

3. **Login falla**
   - Verificar credenciales en `cypress.config.ts`
   - Verificar que el backend esté corriendo
   - Verificar que el usuario exista en la base de datos

4. **Tests inconsistentes**
   - Aumentar retries en `cypress.config.ts`
   - Agregar waits explícitos
   - Verificar estado de la base de datos

---

## 📊 Métricas de Cobertura (Objetivo)

- ✅ Autenticación: Login, Logout básicos
- ⚠️ Admin - Productos: Básico (expandir)
- ⚠️ POS: Básico (expandir)
- ⚠️ Ventas: Básico (expandir)
- ⚠️ Dashboard: Básico (expandir)
- ⚠️ Responsive: Móvil básico (expandir)

---

## 🎓 Mejores Prácticas Implementadas

1. ✅ Uso de Page Objects Pattern
2. ✅ Custom Commands reutilizables
3. ✅ Fixtures para datos de prueba
4. ✅ Atributos data-cy para selectores estables
5. ✅ Separación de tests por módulo
6. ✅ Configuración centralizada
7. ✅ Retries automáticos para tests flaky

---

## 📚 Recursos Adicionales

- [Documentación Cypress](https://docs.cypress.io/)
- [Cypress Best Practices](https://docs.cypress.io/guides/references/best-practices)
- [Page Objects Pattern](https://docs.cypress.io/guides/references/best-practices#Organizing-Tests)

---

## 📅 Historial de Cambios

### Versión 1.0 - Setup Inicial (Fecha: 2025-03-11)
- ✅ Configuración inicial de Cypress
- ✅ Estructura de carpetas
- ✅ Custom commands básicos
- ✅ Page Objects para módulos principales
- ✅ Tests básicos para Auth, Products, POS, Sales, Dashboard
- ✅ Atributos data-cy en componentes principales
- ✅ Scripts npm para ejecución
- ✅ Documentación inicial

---

## 👥 Notas del Desarrollador

Este sistema de testing fue diseñado siguiendo las mejores prácticas de testing E2E con Cypress. La estructura está preparada para escalar fácilmente agregando más tests y mejorando la cobertura.

**Importante**: Asegúrate de mantener actualizados los atributos `data-cy` cuando modifiques componentes. Estos atributos son críticos para la estabilidad de los tests.

---

## 🔗 Enlaces Útiles

- Backend API: http://localhost:5000/swagger
- Frontend: http://localhost:4200
- Cypress Dashboard: (Requiere cuenta Cypress)

---

**Última actualización**: 2025-03-11
**Versión**: 1.0.0
**Estado**: ✅ Setup Completo - Listo para expandir

