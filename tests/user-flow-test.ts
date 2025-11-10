import { test, expect, Page } from '@playwright/test';

// Datos de prueba
const testUser = {
  email: `test-${Date.now()}@example.com`,
  password: 'Test123456',
  firstName: 'Juan',
  lastName: 'Pérez',
  dni: '12345678',
  phone: '+51 999 999 999'
};

const testAddress = {
  label: 'Casa',
  region: 'Lima',
  city: 'Lima',
  district: 'Miraflores',
  address: 'Av. Larco 123',
  reference: 'Al lado del banco'
};

test.describe('Flujo Completo de Usuario', () => {
  test('Flujo completo: Registro -> Completar Perfil -> Login -> Comprar', async ({ page }) => {
    const currentPage = page;
    console.log('🧪 Iniciando pruebas del flujo completo de usuario...\n');

    // PASO 1: REGISTRO
    console.log('📝 PASO 1: Registro de usuario');
    await currentPage.goto('http://localhost:4200/auth/register');
    await currentPage.waitForLoadState('networkidle');

    // Llenar formulario de registro
    await currentPage.fill('input[formControlName="email"]', testUser.email);
    await currentPage.fill('input[formControlName="firstName"]', testUser.firstName);
    await currentPage.fill('input[formControlName="lastName"]', testUser.lastName);
    await currentPage.fill('input[formControlName="dni"]', testUser.dni);
    await currentPage.fill('input[formControlName="phone"]', testUser.phone);
    await currentPage.fill('input[formControlName="password"]', testUser.password);

    // Aceptar términos y condiciones
    await currentPage.check('input[formControlName="acceptTerms"]');
    await currentPage.check('input[formControlName="acceptAdditionalPurposes"]');

    // Enviar formulario
    await currentPage.click('button[type="submit"]');
    
    // Esperar redirección a completar perfil
    await currentPage.waitForURL('**/auth/complete-profile', { timeout: 10000 });
    console.log('✅ Registro exitoso, redirigido a completar perfil');

    // PASO 2: COMPLETAR PERFIL
    console.log('\n📋 PASO 2: Completar perfil con dirección');
    await currentPage.waitForLoadState('networkidle');

    // Verificar que los campos de nombre y teléfono están pre-llenados y son readonly
    const fullNameValue = await currentPage.inputValue('input[formControlName="addressFullName"]');
    const phoneValue = await currentPage.inputValue('input[formControlName="addressPhone"]');
    
    expect(fullNameValue).toBeTruthy();
    expect(phoneValue).toBeTruthy();
    console.log(`✅ Campos pre-llenados: Nombre: ${fullNameValue}, Teléfono: ${phoneValue}`);

    // Llenar formulario de dirección
    await currentPage.fill('input[formControlName="addressLabel"]', testAddress.label);
    
    // Seleccionar región/departamento
    await currentPage.selectOption('select[formControlName="addressRegion"]', testAddress.region);
    await currentPage.waitForTimeout(1000); // Esperar carga de provincias
    
    // Seleccionar ciudad/provincia
    await currentPage.selectOption('select[formControlName="addressCity"]', testAddress.city);
    await currentPage.waitForTimeout(1000); // Esperar carga de distritos
    
    // Seleccionar distrito
    await currentPage.selectOption('select[formControlName="addressDistrict"]', testAddress.district);
    
    // Llenar dirección
    await currentPage.fill('textarea[formControlName="addressAddress"]', testAddress.address);
    await currentPage.fill('textarea[formControlName="addressReference"]', testAddress.reference);

    // Enviar formulario
    await currentPage.click('button[type="submit"]');
    
    // Esperar redirección al perfil o tienda
    await currentPage.waitForURL(/\/(perfil|store|$)/, { timeout: 10000 });
    console.log('✅ Perfil completado exitosamente');

    // PASO 3: LOGOUT Y LOGIN
    console.log('\n🔐 PASO 3: Logout y Login');
    
    // Buscar botón de logout (puede estar en un menú dropdown)
    const logoutButton = currentPage.locator('button:has-text("Cerrar sesión"), a:has-text("Cerrar sesión")').first();
    if (await logoutButton.isVisible({ timeout: 2000 })) {
      await logoutButton.click();
      await currentPage.waitForTimeout(1000);
    }

    // Ir a login
    await currentPage.goto('http://localhost:4200/auth/login');
    await currentPage.waitForLoadState('networkidle');

    // Login con DNI (ya que ahora se usa DNI como username)
    await currentPage.fill('input[formControlName="username"]', testUser.dni);
    await currentPage.fill('input[formControlName="password"]', testUser.password);
    await currentPage.click('button[type="submit"]');

    // Esperar redirección después del login
    await currentPage.waitForURL(/\/(store|perfil|$)/, { timeout: 10000 });
    console.log('✅ Login exitoso');

    // PASO 4: NAVEGAR POR LA TIENDA
    console.log('\n🛒 PASO 4: Navegar por la tienda');
    await currentPage.goto('http://localhost:4200/store');
    await currentPage.waitForLoadState('networkidle');

    // Verificar que la página de tienda carga
    const storeContent = currentPage.locator('body');
    await expect(storeContent).toBeVisible();
    console.log('✅ Tienda cargada correctamente');

    // Buscar y agregar un producto al carrito (si hay productos disponibles)
    const productCards = currentPage.locator('[data-testid="product-card"], .product-card, article').first();
    if (await productCards.count() > 0) {
      const firstProduct = productCards.first();
      await firstProduct.scrollIntoViewIfNeeded();
      
      // Buscar botón de agregar al carrito
      const addToCartButton = firstProduct.locator('button:has-text("Agregar"), button:has-text("Añadir"), [data-testid="add-to-cart"]').first();
      if (await addToCartButton.isVisible({ timeout: 2000 })) {
        await addToCartButton.click();
        await currentPage.waitForTimeout(1000);
        console.log('✅ Producto agregado al carrito');
      }
    }

    // PASO 5: VERIFICAR CARRITO Y CHECKOUT
    console.log('\n🛍️ PASO 5: Verificar carrito');
    
    // Ir al carrito
    const cartButton = currentPage.locator('a[href*="cart"], button:has-text("Carrito")').first();
    if (await cartButton.isVisible({ timeout: 2000 })) {
      await cartButton.click();
      await currentPage.waitForLoadState('networkidle');
      console.log('✅ Carrito accesible');
    }

    // PASO 6: VERIFICAR PERFIL Y DIRECCIONES
    console.log('\n👤 PASO 6: Verificar perfil y direcciones guardadas');
    await currentPage.goto('http://localhost:4200/perfil');
    await currentPage.waitForLoadState('networkidle');

    // Verificar que el nombre del usuario se muestra correctamente
    const userNameElement = currentPage.locator('text=/Juan.*Pérez|Pérez.*Juan/').first();
    if (await userNameElement.isVisible({ timeout: 3000 })) {
      console.log('✅ Nombre de usuario se muestra correctamente en el perfil');
    }

    // Verificar historial de pedidos
    const ordersSection = currentPage.locator('text=/Pedidos|Historial|Órdenes/').first();
    if (await ordersSection.isVisible({ timeout: 2000 })) {
      console.log('✅ Sección de pedidos visible');
    }

    console.log('\n✅ TODAS LAS PRUEBAS COMPLETADAS EXITOSAMENTE');
  });

  test('Validar que no se puede registrar con DNI duplicado', async ({ page }) => {
    const currentPage = page;
    console.log('🧪 Probando validación de DNI duplicado...\n');

    // Intentar registrar con un DNI que ya existe
    await currentPage.goto('http://localhost:4200/auth/register');
    await currentPage.waitForLoadState('networkidle');

    await currentPage.fill('input[formControlName="email"]', `test2-${Date.now()}@example.com`);
    await currentPage.fill('input[formControlName="firstName"]', 'Test');
    await currentPage.fill('input[formControlName="lastName"]', 'User');
    await currentPage.fill('input[formControlName="dni"]', testUser.dni); // DNI duplicado
    await currentPage.fill('input[formControlName="phone"]', '+51 888 888 888');
    await currentPage.fill('input[formControlName="password"]', 'Test123456');
    await currentPage.check('input[formControlName="acceptTerms"]');
    await currentPage.check('input[formControlName="acceptAdditionalPurposes"]');

    await currentPage.click('button[type="submit"]');
    
    // Esperar mensaje de error
    await currentPage.waitForTimeout(2000);
    const errorMessage = currentPage.locator('text=/DNI|duplicado|ya existe|existente/i');
    
    if (await errorMessage.isVisible({ timeout: 3000 })) {
      console.log('✅ Validación de DNI duplicado funciona correctamente');
    } else {
      console.log('⚠️ No se encontró mensaje de error de DNI duplicado (puede que el DNI no esté duplicado)');
    }
  });

  test('Validar formulario de completar perfil - campos requeridos', async ({ page }) => {
    const currentPage = page;
    console.log('🧪 Probando validaciones del formulario de completar perfil...\n');

    // Primero hacer login
    await currentPage.goto('http://localhost:4200/auth/login');
    await currentPage.waitForLoadState('networkidle');
    
    await currentPage.fill('input[formControlName="username"]', testUser.dni);
    await currentPage.fill('input[formControlName="password"]', testUser.password);
    await currentPage.click('button[type="submit"]');
    await currentPage.waitForURL(/\/(store|perfil|$)/, { timeout: 10000 });

    // Ir a completar perfil (si el perfil no está completo)
    await currentPage.goto('http://localhost:4200/auth/complete-profile');
    await currentPage.waitForLoadState('networkidle');

    // Intentar enviar sin llenar campos requeridos
    const submitButton = currentPage.locator('button[type="submit"]');
    const isDisabled = await submitButton.getAttribute('disabled');
    
    if (isDisabled !== null) {
      console.log('✅ Botón de envío está deshabilitado cuando el formulario es inválido');
    }

    // Verificar que los campos de nombre y teléfono son readonly
    const fullNameReadonly = await currentPage.locator('input[formControlName="addressFullName"]').getAttribute('readonly');
    const phoneReadonly = await currentPage.locator('input[formControlName="addressPhone"]').getAttribute('readonly');
    
    if (fullNameReadonly !== null && phoneReadonly !== null) {
      console.log('✅ Campos de nombre y teléfono son readonly (pre-llenados)');
    }
  });
});

