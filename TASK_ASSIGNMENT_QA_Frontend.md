# TASK ASSIGNMENT - QA Frontend - Cypress E2E Testing

**Fecha**: [Fecha Actual]  
**Agente**: @QA-Frontend  
**Prioridad**: 🟠 ALTA  
**Deadline**: Esta semana (4 días hábiles)

---

## CONTEXTO Y OBJETIVO

Como QA Frontend, eres responsable de garantizar la calidad del frontend mediante tests end-to-end con Cypress. El frontend actualmente tiene **0% de coverage en tests E2E**, lo cual es crítico para validar flujos completos de usuario.

**Objetivo**: Implementar suite completa de tests E2E con Cypress que cubra los flujos críticos del sistema, especialmente POS y CRUD operations.

---

## RESPONSABILIDADES DE QA FRONTEND

### 1. E2E Testing con Cypress
- Escribir tests para flujos críticos de usuario
- Validar integración frontend-backend
- Verificar comportamiento de UI

### 2. Test Coverage
- Cubrir flujos críticos (POS, Login, CRUD)
- Validar casos happy path y error paths
- Testear en diferentes navegadores/ambientes

### 3. Quality Assurance
- Reportar bugs encontrados durante testing
- Validar que UX/UI funciona correctamente
- Verificar responsive design

---

## TAREAS ASIGNADAS

### TAREA 1: Setup Cypress (Día 1 - 2 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Cypress configurado y funcionando

#### Acceptance Criteria:
- [ ] Instalar Cypress: `npm install --save-dev cypress`
- [ ] Configurar Cypress en proyecto
- [ ] Crear estructura de carpetas para tests
- [ ] Configurar `cypress.config.ts`
- [ ] Configurar baseUrl en cypress.config
- [ ] Crear comandos personalizados si es necesario
- [ ] Crear fixtures para datos de prueba
- [ ] Verificar que Cypress se abre correctamente
- [ ] Ejecutar test de ejemplo exitosamente

#### Reference Files:
- `minimarket-web/package.json`
- `minimarket-web/cypress.config.ts` (crear)

#### Implementation:
```typescript
// cypress.config.ts
import { defineConfig } from 'cypress'

export default defineConfig({
  e2e: {
    baseUrl: 'http://localhost:4200',
    setupNodeEvents(on, config) {
      // implement node event listeners here
    },
    specPattern: 'cypress/e2e/**/*.cy.{js,jsx,ts,tsx}',
    supportFile: 'cypress/support/e2e.ts',
    viewportWidth: 1280,
    viewportHeight: 720,
    video: true,
    screenshotOnRunFailure: true,
  },
})
```

---

### TAREA 2: E2E Tests - Login Flow (Día 1 - 3 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Tests completos para flujo de autenticación

#### Acceptance Criteria:
- [ ] Test: Login exitoso con credenciales válidas
- [ ] Test: Login fallido con credenciales inválidas
- [ ] Test: Login fallido con usuario inexistente
- [ ] Test: Redirección después de login exitoso
- [ ] Test: Token almacenado correctamente
- [ ] Test: Logout funciona correctamente
- [ ] Test: Acceso a rutas protegidas sin login redirige a login

#### Test Structure:
```typescript
// cypress/e2e/auth/login.cy.ts
describe('Login Flow', () => {
  beforeEach(() => {
    cy.visit('/login');
  });

  it('should login successfully with valid credentials', () => {
    cy.get('[data-cy=username-input]').type('admin');
    cy.get('[data-cy=password-input]').type('Admin123!');
    cy.get('[data-cy=login-button]').click();
    
    cy.url().should('include', '/dashboard');
    cy.get('[data-cy=user-menu]').should('be.visible');
  });

  it('should show error with invalid credentials', () => {
    cy.get('[data-cy=username-input]').type('admin');
    cy.get('[data-cy=password-input]').type('wrongpassword');
    cy.get('[data-cy=login-button]').click();
    
    cy.get('[data-cy=error-message]').should('be.visible');
    cy.get('[data-cy=error-message]').should('contain', 'Credenciales inválidas');
  });

  // Más tests...
});
```

---

### TAREA 3: E2E Tests - POS Flow (Día 2-3 - 6 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Tests completos para flujo completo de POS

#### Acceptance Criteria:
- [ ] Test: Búsqueda de producto funciona
- [ ] Test: Agregar producto al carrito funciona
- [ ] Test: Modificar cantidad en carrito funciona
- [ ] Test: Eliminar producto del carrito funciona
- [ ] Test: Cálculos automáticos (subtotal, IGV, total) son correctos
- [ ] Test: Selección de tipo de comprobante (Boleta/Factura)
- [ ] Test: Búsqueda y selección de cliente para factura
- [ ] Test: Selección de método de pago
- [ ] Test: Cálculo de vuelto para efectivo
- [ ] Test: Procesar venta exitosamente
- [ ] Test: Validación de stock insuficiente muestra error
- [ ] Test: Validación de factura sin cliente muestra error
- [ ] Test: Validación de monto pagado insuficiente muestra error
- [ ] Test: Stock se actualiza después de venta
- [ ] Test: Carrito se limpia después de venta exitosa

#### Test Structure:
```typescript
// cypress/e2e/pos/pos-flow.cy.ts
describe('POS Flow', () => {
  beforeEach(() => {
    // Login primero
    cy.login('cajero', 'Cajero123!');
    cy.visit('/pos');
  });

  it('should complete a sale successfully', () => {
    // Buscar producto
    cy.get('[data-cy=product-search]').type('Producto Test');
    cy.get('[data-cy=product-item]').first().click();
    
    // Verificar producto en carrito
    cy.get('[data-cy=cart-item]').should('have.length', 1);
    
    // Verificar cálculos
    cy.get('[data-cy=subtotal]').should('contain', '100.00');
    cy.get('[data-cy=tax]').should('contain', '18.00');
    cy.get('[data-cy=total]').should('contain', '118.00');
    
    // Seleccionar método de pago
    cy.get('[data-cy=payment-method]').select('Efectivo');
    cy.get('[data-cy=amount-paid]').type('150');
    
    // Verificar vuelto
    cy.get('[data-cy=change]').should('contain', '32.00');
    
    // Procesar venta
    cy.get('[data-cy=process-sale-button]').click();
    
    // Verificar éxito
    cy.get('[data-cy=success-toast]').should('be.visible');
    cy.get('[data-cy=cart-items]').should('have.length', 0);
  });

  it('should show error when stock is insufficient', () => {
    // Agregar producto con cantidad mayor al stock
    cy.get('[data-cy=product-item]').first().click();
    cy.get('[data-cy=quantity-input]').clear().type('9999');
    
    // Intentar procesar
    cy.get('[data-cy=process-sale-button]').click();
    
    // Verificar error
    cy.get('[data-cy=error-toast]').should('be.visible');
    cy.get('[data-cy=error-toast]').should('contain', 'Stock insuficiente');
  });

  // Más tests...
});
```

---

### TAREA 4: E2E Tests - Products CRUD (Día 3 - 4 horas)

**PRIORITY**: 🟠 ALTA  
**DELIVERABLE**: Tests para CRUD de productos

#### Acceptance Criteria:
- [ ] Test: Listar productos funciona
- [ ] Test: Búsqueda de productos funciona
- [ ] Test: Filtro por categoría funciona
- [ ] Test: Crear producto exitosamente
- [ ] Test: Validaciones de formulario funcionan
- [ ] Test: Editar producto funciona
- [ ] Test: Eliminar producto funciona
- [ ] Test: Confirmación antes de eliminar

#### Test Structure:
```typescript
// cypress/e2e/products/products-crud.cy.ts
describe('Products CRUD', () => {
  beforeEach(() => {
    cy.login('admin', 'Admin123!');
    cy.visit('/productos');
  });

  it('should create a new product', () => {
    cy.get('[data-cy=create-product-button]').click();
    
    cy.get('[data-cy=product-code]').type('TEST001');
    cy.get('[data-cy=product-name]').type('Producto Test');
    cy.get('[data-cy=product-purchase-price]').type('10');
    cy.get('[data-cy=product-sale-price]').type('15');
    cy.get('[data-cy=product-stock]').type('100');
    cy.get('[data-cy=product-category]').select('1');
    
    cy.get('[data-cy=save-button]').click();
    
    cy.get('[data-cy=success-toast]').should('be.visible');
    cy.url().should('include', '/productos');
  });

  it('should show validation errors for invalid data', () => {
    cy.get('[data-cy=create-product-button]').click();
    
    cy.get('[data-cy=product-sale-price]').type('5');
    cy.get('[data-cy=product-purchase-price]').type('10');
    
    cy.get('[data-cy=save-button]').click();
    
    cy.get('[data-cy=error-message]').should('contain', 'precio de venta debe ser mayor');
  });

  // Más tests...
});
```

---

### TAREA 5: E2E Tests - Customers CRUD (Día 4 - 3 horas)

**PRIORITY**: 🟡 MEDIA  
**DELIVERABLE**: Tests para CRUD de clientes

#### Acceptance Criteria:
- [ ] Test: Listar clientes funciona
- [ ] Test: Búsqueda de clientes funciona
- [ ] Test: Crear cliente exitosamente
- [ ] Test: Validación de DNI (8 dígitos)
- [ ] Test: Validación de RUC (11 dígitos)
- [ ] Test: Validación de teléfono peruano
- [ ] Test: Validación de documento duplicado
- [ ] Test: Editar cliente funciona
- [ ] Test: Eliminar cliente funciona

---

### TAREA 6: E2E Tests - Sales History (Día 4 - 2 horas)

**PRIORITY**: 🟡 MEDIA  
**DELIVERABLE**: Tests para historial de ventas

#### Acceptance Criteria:
- [ ] Test: Listar ventas funciona
- [ ] Test: Ver detalle de venta funciona
- [ ] Test: Filtros de ventas funcionan
- [ ] Test: Anular venta funciona
- [ ] Test: Reimprimir comprobante funciona (si está implementado)

---

## ESTRUCTURA DE CARPETAS

```
minimarket-web/
├── cypress/
│   ├── e2e/
│   │   ├── auth/
│   │   │   └── login.cy.ts
│   │   ├── pos/
│   │   │   ├── pos-flow.cy.ts
│   │   │   └── pos-calculations.cy.ts
│   │   ├── products/
│   │   │   └── products-crud.cy.ts
│   │   ├── customers/
│   │   │   └── customers-crud.cy.ts
│   │   └── sales/
│   │       └── sales-history.cy.ts
│   ├── fixtures/
│   │   ├── users.json
│   │   ├── products.json
│   │   └── customers.json
│   ├── support/
│   │   ├── commands.ts (comandos personalizados)
│   │   └── e2e.ts
│   └── config.ts
```

---

## ESTÁNDARES DE TESTING

### Naming Conventions
- **Test Files**: `[feature].cy.ts`
- **Test Suites**: `describe('Feature Name', ...)`
- **Test Cases**: `it('should [action] when [condition]', ...)`

### Data Attributes
- Usar `data-cy` attributes para selectores
- Ejemplo: `data-cy="product-search"`, `data-cy="cart-item"`

### Best Practices
- **Arrange-Act-Assert**: Estructura clara
- **Page Object Pattern**: Considerar para tests complejos
- **Custom Commands**: Para acciones repetitivas (login, etc.)
- **Fixtures**: Para datos de prueba

### Custom Commands
```typescript
// cypress/support/commands.ts
declare global {
  namespace Cypress {
    interface Chainable {
      login(username: string, password: string): Chainable<void>;
    }
  }
}

Cypress.Commands.add('login', (username: string, password: string) => {
  cy.visit('/login');
  cy.get('[data-cy=username-input]').type(username);
  cy.get('[data-cy=password-input]').type(password);
  cy.get('[data-cy=login-button]').click();
  cy.url().should('include', '/dashboard');
});
```

---

## MÉTRICAS Y OBJETIVOS

### Test Count Targets
- **Login Tests**: Mínimo 5 tests
- **POS Tests**: Mínimo 10 tests
- **Products CRUD Tests**: Mínimo 8 tests
- **Customers CRUD Tests**: Mínimo 6 tests
- **Sales History Tests**: Mínimo 4 tests
- **Total**: Mínimo 33 tests E2E

### Quality Metrics
- **Tests Passing**: 100%
- **Test Execution Time**: <5 minutos para suite completa
- **Test Coverage**: Flujos críticos 100% cubiertos

---

## DEPENDENCIAS Y BLOQUEOS

### Dependencias
- ✅ Frontend implementado
- ✅ Backend API funcionando
- ⚠️ Necesita que UX/UI agregue data-cy attributes si no existen

### Bloqueos Potenciales
- Si falta data-cy attributes en componentes
- Si hay problemas con configuración de Cypress
- Si backend no está disponible para tests

### Acción si Bloqueado
- Reportar inmediatamente a Tech Lead
- Solicitar a UX/UI que agregue data-cy attributes
- Usar selectores alternativos temporalmente

---

## REPORTE DIARIO REQUERIDO

Al final de cada día, reportar:

```
## DAILY PROGRESS - QA Frontend - [Fecha]

### Tests Escritos Hoy:
- Login: X tests
- POS: Y tests
- CRUD: Z tests
- Total: X + Y + Z tests

### Tests Passing:
- ✅ Todos pasando / ⚠️ X tests fallando

### Blockers:
- [Lista de blockers si los hay]

### Plan Mañana:
- [Tareas específicas para mañana]
```

---

## ACCEPTANCE CRITERIA FINAL

El trabajo está **COMPLETO** cuando:

- [ ] ✅ Cypress configurado y funcionando
- [ ] ✅ Tests E2E para login completos
- [ ] ✅ Tests E2E para POS completos
- [ ] ✅ Tests E2E para CRUD completos
- [ ] ✅ Todos los tests pasan (green)
- [ ] ✅ Tests son estables y no flaky
- [ ] ✅ Custom commands creados para acciones comunes
- [ ] ✅ Fixtures configurados para datos de prueba
- [ ] ✅ Documentación de tests actualizada
- [ ] ✅ PR creado con todos los tests
- [ ] ✅ Code review aprobado por Tech Lead

---

## RECURSOS Y REFERENCIAS

### Documentación
- [Cypress Documentation](https://docs.cypress.io/)
- [Cypress Best Practices](https://docs.cypress.io/guides/references/best-practices)
- [Cypress Custom Commands](https://docs.cypress.io/api/cypress-api/custom-commands)

### Archivos de Referencia
- `minimarket-web/src/app/features/` - Componentes a testear
- `minimarket-web/src/app/core/services/` - Servicios a validar

---

## PRIORIZACIÓN DE TAREAS

**Orden de Ejecución Recomendado**:
1. **Día 1**: Tarea 1 (Setup) → Tarea 2 (Login)
2. **Día 2**: Tarea 3 (POS - inicio)
3. **Día 3**: Tarea 3 (POS - completar) → Tarea 4 (Products CRUD)
4. **Día 4**: Tarea 5 (Customers CRUD) → Tarea 6 (Sales History)

---

## NOTAS FINALES

**@QA-Frontend**: 

Esta tarea es **ALTA PRIORIDAD** porque valida que los flujos críticos funcionan end-to-end. Los tests E2E son la última línea de defensa antes de producción.

**ENFÓCATE EN**:
- ✅ Flujos críticos primero (POS, Login)
- ✅ Tests estables (no flaky)
- ✅ Validar integración frontend-backend
- ✅ Casos happy path y error paths

**ESTA TAREA ES COMPLEMENTARIA A QA-Backend Y CRÍTICA PARA CALIDAD DEL FRONTEND.**

---

**ASIGNADO POR**: Tech Lead  
**FECHA**: [Fecha Actual]  
**DEADLINE**: [Fecha + 4 días hábiles]  
**STATUS**: 🟡 EN PROGRESO

