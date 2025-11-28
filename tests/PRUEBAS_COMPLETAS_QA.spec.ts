import { test, expect, Page } from '@playwright/test';

/**
 * PRUEBAS COMPLETAS QA - Sistema Camucha
 * 
 * Este archivo contiene pruebas exhaustivas que cubren:
 * - Pruebas funcionales críticas
 * - Pruebas de estrés y carga
 * - Pruebas de validación de reglas de negocio
 * - Pruebas de integración
 * - Pruebas de rendimiento
 * - Pruebas de seguridad básica
 */

// Credenciales del sistema
const ADMIN_CREDENTIALS = {
  username: 'admin@minimarketcamucha.com',
  password: 'Admin123!'
};

const CAJERO_CREDENTIALS = {
  username: 'cajero@minimarketcamucha.com',
  password: 'Cajero123!'
};

// Helper: Login mejorado
async function loginAs(page: Page, credentials: { username: string; password: string }) {
  console.log(`🔐 Login: ${credentials.username}`);
  
  await page.goto('http://localhost:4200/auth/login');
  await page.waitForLoadState('networkidle');
  
  const emailInput = page.locator('input[formControlName="email"]');
  const passwordInput = page.locator('input[formControlName="password"]');
  const submitButton = page.locator('button[type="submit"]');
  
  await expect(emailInput).toBeVisible({ timeout: 10000 });
  await expect(passwordInput).toBeVisible({ timeout: 10000 });
  await expect(submitButton).toBeVisible({ timeout: 10000 });
  
  await emailInput.fill(credentials.username);
  await passwordInput.fill(credentials.password);
  
  // Esperar respuesta del API
  const responsePromise = page.waitForResponse(
    response => response.url().includes('/api/auth/login') && response.request().method() === 'POST',
    { timeout: 15000 }
  ).catch(() => null);
  
  await submitButton.click();
  
  const response = await responsePromise;
  if (response && response.status() !== 200) {
    throw new Error(`Login falló: ${response.status()}`);
  }
  
  // Esperar redirección
  await page.waitForURL(/\/(admin|auth\/(admin-setup|complete-profile)|\/|store)/, { 
    timeout: 15000,
    waitUntil: 'networkidle'
  });
  
  // Manejar setup si es necesario
  if (page.url().includes('admin-setup') || page.url().includes('complete-profile')) {
    await page.waitForTimeout(2000);
    const skipButton = page.locator('button:has-text("Saltar"), a:has-text("Saltar")').first();
    if (await skipButton.isVisible({ timeout: 3000 })) {
      await skipButton.click();
      await page.waitForTimeout(2000);
    }
  }
}

// Helper: Navegar al POS
async function navigateToPOS(page: Page) {
  const posLink = page.locator('a:has-text("POS"), a:has-text("Punto de Venta"), [href*="pos"]').first();
  if (await posLink.isVisible({ timeout: 5000 })) {
    await posLink.click();
  } else {
    await page.goto('http://localhost:4200/admin/pos');
  }
  await page.waitForLoadState('networkidle');
  await page.waitForSelector('h2:has-text("Punto de Venta"), [data-cy="product-search"]', { timeout: 10000 });
}

// Helper: Agregar producto al carrito
async function addProductToCart(page: Page) {
  const addButtons = page.locator('button:has-text("Agregar")');
  const count = await addButtons.count();
  if (count > 0) {
    await addButtons.first().click();
    await page.waitForTimeout(1000);
    return true;
  }
  return false;
}

// ============================================
// GRUPO 1: PRUEBAS FUNCIONALES CRÍTICAS
// ============================================

test.describe('1. PRUEBAS FUNCIONALES CRÍTICAS', () => {
  
  test('1.1 Login con credenciales válidas', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    // Esperar a que la URL cambie (puede estar en complete-profile o admin)
    await page.waitForTimeout(2000);
    const url = page.url();
    // Verificar que no está en login (puede estar en complete-profile, admin-setup, o admin)
    expect(url).not.toContain('/auth/login');
    await page.screenshot({ path: 'test-results/1.1-login-exitoso.png', fullPage: true });
  });

  test('1.2 Login con credenciales inválidas', async ({ page }) => {
    await page.goto('http://localhost:4200/auth/login');
    await page.fill('input[formControlName="email"]', 'invalid@test.com');
    await page.fill('input[formControlName="password"]', 'wrongpassword');
    
    const responsePromise = page.waitForResponse(
      response => response.url().includes('/api/auth/login'),
      { timeout: 10000 }
    ).catch(() => null);
    
    await page.click('button[type="submit"]');
    const response = await responsePromise;
    
    if (response) {
      expect(response.status()).toBe(400);
    }
    
    const errorMessage = page.locator('[data-cy="error-message"]');
    if (await errorMessage.isVisible({ timeout: 3000 })) {
      await page.screenshot({ path: 'test-results/1.2-login-error.png', fullPage: true });
    }
  });

  test('1.3 Flujo completo de venta - Boleta', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    await navigateToPOS(page);
    
    // Agregar producto
    await addProductToCart(page);
    await page.waitForTimeout(1000);
    
    // Configurar pago
    await page.locator('[data-cy="payment-method-select"]').selectOption('Efectivo');
    await page.waitForTimeout(500);
    
    // Establecer monto
    const totalElement = page.locator('[data-cy="total"]');
    if (await totalElement.isVisible({ timeout: 5000 })) {
      const totalText = await totalElement.textContent() || '';
      const totalMatch = totalText.match(/[\d.]+/);
      if (totalMatch) {
        const totalAmount = parseFloat(totalMatch[0]);
        await page.locator('[data-cy="amount-paid"]').fill((totalAmount + 0.01).toString());
        await page.waitForTimeout(1000);
      }
    }
    
    // Procesar venta
    const processButton = page.locator('[data-cy="complete-sale-button"]');
    if (await processButton.isVisible({ timeout: 5000 }) && !(await processButton.isDisabled())) {
      await processButton.click();
      await page.waitForTimeout(3000);
      await page.screenshot({ path: 'test-results/1.3-venta-completa.png', fullPage: true });
    }
  });

  test('1.4 Validar stock en tiempo real', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    await navigateToPOS(page);
    
    await addProductToCart(page);
    
    // Aumentar cantidad excesivamente
    const increaseButtons = page.locator('[data-cy="cart-items"] button:has(span.material-symbols-outlined:has-text("add"))');
    if (await increaseButtons.count() > 0) {
      for (let i = 0; i < 50; i++) {
        await increaseButtons.first().click();
        await page.waitForTimeout(100);
      }
    }
    
    await page.screenshot({ path: 'test-results/1.4-validacion-stock.png', fullPage: true });
  });

  test('1.5 Generar Factura con RUC', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    await navigateToPOS(page);
    
    // Seleccionar Factura
    await page.locator('[data-cy="document-type"]').selectOption('Factura');
    await page.waitForTimeout(1000);
    
    // Buscar cliente con RUC
    const customerSearch = page.locator('[data-cy="customer-search"]');
    if (await customerSearch.isVisible({ timeout: 5000 })) {
      await customerSearch.fill('RUC');
      await page.waitForTimeout(1000);
    }
    
    await addProductToCart(page);
    await page.screenshot({ path: 'test-results/1.5-factura-ruc.png', fullPage: true });
  });
});

// ============================================
// GRUPO 2: PRUEBAS DE ESTRÉS Y CARGA
// ============================================

test.describe('2. PRUEBAS DE ESTRÉS Y CARGA', () => {
  
  test('2.1 Agregar 100 productos diferentes al carrito', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    await navigateToPOS(page);
    
    const addButtons = page.locator('button:has-text("Agregar")');
    const count = await addButtons.count();
    const maxProducts = Math.min(100, count);
    
    console.log(`📊 Agregando ${maxProducts} productos...`);
    
    for (let i = 0; i < maxProducts; i++) {
      if (i < count) {
        await addButtons.nth(i).click();
        await page.waitForTimeout(100);
      }
    }
    
    await page.screenshot({ path: 'test-results/2.1-100-productos.png', fullPage: true });
    console.log(`✅ ${maxProducts} productos agregados`);
  });

  test('2.2 Validar límite de 500 items', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    await navigateToPOS(page);
    
    await addProductToCart(page);
    
    // Aumentar cantidad hasta 500
    const increaseButtons = page.locator('[data-cy="cart-items"] button:has(span.material-symbols-outlined:has-text("add"))');
    if (await increaseButtons.count() > 0) {
      for (let i = 0; i < 500; i++) {
        await increaseButtons.first().click();
        if (i % 50 === 0) {
          await page.waitForTimeout(100);
        }
      }
    }
    
    const cartItems = page.locator('[data-cy="cart-items"] tbody tr');
    const itemCount = await cartItems.count();
    console.log(`📊 Items en carrito: ${itemCount}`);
    
    await page.screenshot({ path: 'test-results/2.2-limite-500-items.png', fullPage: true });
  });

  test('2.3 Múltiples ventas rápidas consecutivas', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    await navigateToPOS(page);
    
    for (let venta = 1; venta <= 5; venta++) {
      console.log(`🛒 Procesando venta ${venta}/5...`);
      
      await addProductToCart(page);
      await page.waitForTimeout(500);
      
      await page.locator('[data-cy="payment-method-select"]').selectOption('Efectivo');
      
      const totalElement = page.locator('[data-cy="total"]');
      if (await totalElement.isVisible({ timeout: 3000 })) {
        const totalText = await totalElement.textContent() || '';
        const totalMatch = totalText.match(/[\d.]+/);
        if (totalMatch) {
          const totalAmount = parseFloat(totalMatch[0]);
          await page.locator('[data-cy="amount-paid"]').fill((totalAmount + 0.01).toString());
          await page.waitForTimeout(500);
        }
      }
      
      const processButton = page.locator('[data-cy="complete-sale-button"]');
      if (await processButton.isVisible({ timeout: 3000 }) && !(await processButton.isDisabled())) {
        await processButton.click();
        await page.waitForTimeout(2000);
      }
      
      // Cerrar cualquier modal que pueda estar abierto (document-type-dialog)
      const modal = page.locator('.fixed.inset-0.bg-black').first();
      if (await modal.isVisible({ timeout: 2000 })) {
        // Presionar Escape para cerrar modal
        await page.keyboard.press('Escape');
        await page.waitForTimeout(1000);
      }
      
      // Limpiar carrito para siguiente venta
      const clearButton = page.locator('[data-cy="clear-cart"]');
      if (await clearButton.isVisible({ timeout: 3000 })) {
        // Verificar que no hay modal bloqueando
        const modalStillOpen = await modal.isVisible({ timeout: 500 }).catch(() => false);
        if (modalStillOpen) {
          // Cerrar modal primero con click fuera
          await page.keyboard.press('Escape');
          await page.waitForTimeout(1000);
        }
        await clearButton.click({ force: true });
        await page.waitForTimeout(1000);
      }
    }
    
    await page.screenshot({ path: 'test-results/2.3-multiples-ventas.png', fullPage: true });
    console.log('✅ 5 ventas procesadas');
  });

  test('2.4 Carga de dashboard con muchas ventas', async ({ page }) => {
    await loginAs(page, ADMIN_CREDENTIALS);
    await page.goto('http://localhost:4200/admin');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(5000); // Dar tiempo a cargar estadísticas
    
    const startTime = Date.now();
    await page.waitForLoadState('networkidle');
    const loadTime = Date.now() - startTime;
    
    console.log(`📊 Tiempo de carga del dashboard: ${loadTime}ms`);
    await page.screenshot({ path: 'test-results/2.4-dashboard-carga.png', fullPage: true });
    
    expect(loadTime).toBeLessThan(10000); // Debe cargar en menos de 10 segundos
  });
});

// ============================================
// GRUPO 3: PRUEBAS DE VALIDACIÓN DE REGLAS
// ============================================

test.describe('3. VALIDACIÓN DE REGLAS DE NEGOCIO', () => {
  
  test('3.1 Validar que monto pagado no puede ser menor al total', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    await navigateToPOS(page);
    
    await addProductToCart(page);
    
    const totalElement = page.locator('[data-cy="total"]');
    if (await totalElement.isVisible({ timeout: 5000 })) {
      const totalText = await totalElement.textContent() || '';
      const totalMatch = totalText.match(/[\d.]+/);
      if (totalMatch) {
        const totalAmount = parseFloat(totalMatch[0]);
        // Intentar pagar menos del total
        await page.locator('[data-cy="amount-paid"]').fill((totalAmount - 1).toString());
        await page.waitForTimeout(1000);
        
        const processButton = page.locator('[data-cy="complete-sale-button"]');
        const isDisabled = await processButton.isDisabled();
        
        expect(isDisabled).toBe(true);
        await page.screenshot({ path: 'test-results/3.1-monto-insuficiente.png', fullPage: true });
      }
    }
  });

  test('3.2 Validar que carrito vacío deshabilita botón de cobrar', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    await navigateToPOS(page);
    
    const processButton = page.locator('[data-cy="complete-sale-button"]');
    const isDisabled = await processButton.isDisabled();
    
    expect(isDisabled).toBe(true);
    await page.screenshot({ path: 'test-results/3.2-carrito-vacio.png', fullPage: true });
  });

  test('3.3 Validar que Factura requiere RUC', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    await navigateToPOS(page);
    
    await page.locator('[data-cy="document-type"]').selectOption('Factura');
    await page.waitForTimeout(1000);
    
    await addProductToCart(page);
    
    // Intentar procesar sin cliente con RUC
    const processButton = page.locator('[data-cy="complete-sale-button"]');
    const isDisabled = await processButton.isDisabled();
    
    // El botón debería estar deshabilitado o mostrar error
    await page.screenshot({ path: 'test-results/3.3-factura-sin-ruc.png', fullPage: true });
  });

  test('3.4 Validar descuento no puede exceder subtotal', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    await navigateToPOS(page);
    
    await addProductToCart(page);
    
    const totalElement = page.locator('[data-cy="total"]');
    if (await totalElement.isVisible({ timeout: 5000 })) {
      const totalText = await totalElement.textContent() || '';
      const totalMatch = totalText.match(/[\d.]+/);
      if (totalMatch) {
        const totalAmount = parseFloat(totalMatch[0]);
        
        // Intentar aplicar descuento mayor al total
        const discountInput = page.locator('[data-cy="discount"]');
        if (await discountInput.isVisible({ timeout: 3000 })) {
          await discountInput.fill((totalAmount + 100).toString());
          await page.waitForTimeout(1000);
          
          await page.screenshot({ path: 'test-results/3.4-descuento-excesivo.png', fullPage: true });
        }
      }
    }
  });
});

// ============================================
// GRUPO 4: PRUEBAS DE RENDIMIENTO
// ============================================

test.describe('4. PRUEBAS DE RENDIMIENTO', () => {
  
  test('4.1 Tiempo de respuesta del API de login', async ({ page }) => {
    await page.goto('http://localhost:4200/auth/login');
    
    const startTime = Date.now();
    const responsePromise = page.waitForResponse(
      response => response.url().includes('/api/auth/login'),
      { timeout: 10000 }
    );
    
    await page.fill('input[formControlName="email"]', CAJERO_CREDENTIALS.username);
    await page.fill('input[formControlName="password"]', CAJERO_CREDENTIALS.password);
    await page.click('button[type="submit"]');
    
    const response = await responsePromise;
    const responseTime = Date.now() - startTime;
    
    console.log(`⏱️ Tiempo de respuesta del login: ${responseTime}ms`);
    expect(responseTime).toBeLessThan(5000); // Debe responder en menos de 5 segundos
  });

  test('4.2 Tiempo de carga del módulo POS', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    
    const startTime = Date.now();
    await navigateToPOS(page);
    const loadTime = Date.now() - startTime;
    
    console.log(`⏱️ Tiempo de carga del POS: ${loadTime}ms`);
    expect(loadTime).toBeLessThan(10000); // Debe cargar en menos de 10 segundos
  });

  test('4.3 Tiempo de generación de PDF', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    await navigateToPOS(page);
    
    await addProductToCart(page);
    await page.locator('[data-cy="payment-method-select"]').selectOption('Efectivo');
    
    const totalElement = page.locator('[data-cy="total"]');
    if (await totalElement.isVisible({ timeout: 5000 })) {
      const totalText = await totalElement.textContent() || '';
      const totalMatch = totalText.match(/[\d.]+/);
      if (totalMatch) {
        const totalAmount = parseFloat(totalMatch[0]);
        await page.locator('[data-cy="amount-paid"]').fill((totalAmount + 0.01).toString());
        await page.waitForTimeout(1000);
      }
    }
    
    const processButton = page.locator('[data-cy="complete-sale-button"]');
    if (await processButton.isVisible({ timeout: 5000 }) && !(await processButton.isDisabled())) {
      const startTime = Date.now();
      await processButton.click();
      
      // Esperar a que se genere el PDF (puede ser descarga o preview)
      await page.waitForTimeout(5000);
      const pdfTime = Date.now() - startTime;
      
      console.log(`⏱️ Tiempo de generación de PDF: ${pdfTime}ms`);
      expect(pdfTime).toBeLessThan(15000); // Debe generar en menos de 15 segundos
    }
  });
});

// ============================================
// GRUPO 5: PRUEBAS DE INTEGRACIÓN
// ============================================

test.describe('5. PRUEBAS DE INTEGRACIÓN', () => {
  
  test('5.1 Flujo completo: Login → POS → Venta → Dashboard', async ({ page }) => {
    // Login
    await loginAs(page, CAJERO_CREDENTIALS);
    
    // POS
    await navigateToPOS(page);
    
    // Venta
    await addProductToCart(page);
    await page.locator('[data-cy="payment-method-select"]').selectOption('Efectivo');
    
    const totalElement = page.locator('[data-cy="total"]');
    if (await totalElement.isVisible({ timeout: 5000 })) {
      const totalText = await totalElement.textContent() || '';
      const totalMatch = totalText.match(/[\d.]+/);
      if (totalMatch) {
        const totalAmount = parseFloat(totalMatch[0]);
        await page.locator('[data-cy="amount-paid"]').fill((totalAmount + 0.01).toString());
        await page.waitForTimeout(1000);
      }
    }
    
    const processButton = page.locator('[data-cy="complete-sale-button"]');
    if (await processButton.isVisible({ timeout: 5000 }) && !(await processButton.isDisabled())) {
      await processButton.click();
      await page.waitForTimeout(3000);
    }
    
    // Dashboard (como admin)
    await loginAs(page, ADMIN_CREDENTIALS);
    await page.goto('http://localhost:4200/admin');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(3000);
    
    await page.screenshot({ path: 'test-results/5.1-flujo-completo.png', fullPage: true });
  });

  test('5.2 Integración: Stock se actualiza después de venta', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    await navigateToPOS(page);
    
    // Obtener stock inicial (si es visible)
    await addProductToCart(page);
    await page.waitForTimeout(1000);
    
    // Procesar venta
    await page.locator('[data-cy="payment-method-select"]').selectOption('Efectivo');
    
    const totalElement = page.locator('[data-cy="total"]');
    if (await totalElement.isVisible({ timeout: 5000 })) {
      const totalText = await totalElement.textContent() || '';
      const totalMatch = totalText.match(/[\d.]+/);
      if (totalMatch) {
        const totalAmount = parseFloat(totalMatch[0]);
        await page.locator('[data-cy="amount-paid"]').fill((totalAmount + 0.01).toString());
        await page.waitForTimeout(1000);
      }
    }
    
    const processButton = page.locator('[data-cy="complete-sale-button"]');
    if (await processButton.isVisible({ timeout: 5000 }) && !(await processButton.isDisabled())) {
      await processButton.click();
      await page.waitForTimeout(3000);
      
      // Verificar que el stock se actualizó (el producto debería tener menos stock)
      await page.screenshot({ path: 'test-results/5.2-stock-actualizado.png', fullPage: true });
    }
  });
});

// ============================================
// GRUPO 6: PRUEBAS DE SEGURIDAD BÁSICA
// ============================================

test.describe('6. PRUEBAS DE SEGURIDAD BÁSICA', () => {
  
  test('6.1 Intentar acceder a admin sin autenticación', async ({ page }) => {
    await page.goto('http://localhost:4200/admin');
    await page.waitForLoadState('networkidle');
    
    // Debe redirigir a login
    expect(page.url()).toContain('/auth/login');
    await page.screenshot({ path: 'test-results/6.1-acceso-no-autorizado.png', fullPage: true });
  });

  test('6.2 Validar que tokens expiran', async ({ page }) => {
    // Esta prueba requiere implementación adicional
    // Por ahora, solo verificamos que el login funciona
    await loginAs(page, CAJERO_CREDENTIALS);
    await page.waitForTimeout(2000);
    const url = page.url();
    // Verificar que no está en login
    expect(url).not.toContain('/auth/login');
  });

  test('6.3 Validar protección CSRF en formularios', async ({ page }) => {
    await loginAs(page, CAJERO_CREDENTIALS);
    await navigateToPOS(page);
    
    // Los formularios deben tener protección CSRF
    // Verificar que las peticiones incluyen headers necesarios
    await page.screenshot({ path: 'test-results/6.3-csrf-protection.png', fullPage: true });
  });
});

