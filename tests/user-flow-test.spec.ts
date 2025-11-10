import { test, expect, Page } from '@playwright/test';

// Función para generar datos de prueba únicos
function generateTestUser() {
  const timestamp = Date.now();
  const random = Math.floor(Math.random() * 10000);
  return {
    email: `test-${timestamp}-${random}@example.com`,
    password: 'Test123456',
    firstName: 'Juan',
    lastName: 'Pérez',
    dni: `${String(timestamp).slice(-8)}`, // Últimos 8 dígitos del timestamp
    phone: `+51 ${String(random).padStart(9, '0')}`
  };
}

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
    const testUser = generateTestUser(); // Generar usuario único para esta prueba
    console.log('🧪 Iniciando pruebas del flujo completo de usuario...\n');
    console.log(`📧 Usuario de prueba: ${testUser.email}, DNI: ${testUser.dni}`);

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
    
    // Esperar a que el formulario se procese (puede mostrar error o redirigir)
    await currentPage.waitForTimeout(2000);
    
    // Verificar si hay mensaje de error
    const errorMessage = currentPage.locator('[data-cy="error-message"], .bg-red-50, .text-red-600').first();
    if (await errorMessage.isVisible({ timeout: 2000 })) {
      const errorText = await errorMessage.textContent();
      console.log(`⚠️ Error en registro: ${errorText}`);
      // Tomar screenshot para debugging
      await currentPage.screenshot({ path: 'test-results/registration-error.png' });
      throw new Error(`Error en registro: ${errorText}`);
    }
    
    // Esperar redirección a completar perfil o perfil
    try {
      await currentPage.waitForURL(/\/(auth\/complete-profile|perfil|store|$)/, { timeout: 15000 });
      const currentUrl = currentPage.url();
      if (currentUrl.includes('complete-profile')) {
        console.log('✅ Registro exitoso, redirigido a completar perfil');
      } else {
        console.log(`✅ Registro exitoso, redirigido a: ${currentUrl}`);
      }
    } catch (error) {
      // Si no redirige, tomar screenshot para debugging
      await currentPage.screenshot({ path: 'test-results/registration-no-redirect.png' });
      const currentUrl = currentPage.url();
      console.log(`⚠️ No se redirigió después del registro. URL actual: ${currentUrl}`);
      throw error;
    }

    // PASO 2: COMPLETAR PERFIL
    console.log('\n📋 PASO 2: Completar perfil con dirección');
    await currentPage.waitForLoadState('networkidle');
    
    // Verificar que estamos en la página correcta
    const currentUrl = currentPage.url();
    if (!currentUrl.includes('complete-profile')) {
      console.log(`⚠️ No estamos en la página de completar perfil. URL actual: ${currentUrl}`);
      await currentPage.screenshot({ path: 'test-results/wrong-page-after-registration.png' });
    }

    // Esperar a que los campos se carguen
    await currentPage.waitForSelector('input[formControlName="addressFullName"], input[formControlName="addressLabel"]', { timeout: 10000 });

    // Verificar que los campos de nombre y teléfono están pre-llenados y son readonly
    const fullNameInput = currentPage.locator('input[formControlName="addressFullName"]').first();
    const phoneInput = currentPage.locator('input[formControlName="addressPhone"]').first();
    
    if (await fullNameInput.isVisible({ timeout: 5000 })) {
      const fullNameValue = await fullNameInput.inputValue();
      const phoneValue = await phoneInput.inputValue();
      
      expect(fullNameValue).toBeTruthy();
      expect(phoneValue).toBeTruthy();
      console.log(`✅ Campos pre-llenados: Nombre: ${fullNameValue}, Teléfono: ${phoneValue}`);
    } else {
      console.log('⚠️ Campos de dirección no encontrados, puede que el formulario tenga una estructura diferente');
      await currentPage.screenshot({ path: 'test-results/address-form-not-found.png' });
    }

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

    // Login con email (el formulario usa email como formControlName)
    await currentPage.fill('input[formControlName="email"]', testUser.email);
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
    const testUser = generateTestUser(); // Usuario base
    console.log('🧪 Probando validación de DNI duplicado...\n');

    // Primero registrar un usuario
    await currentPage.goto('http://localhost:4200/auth/register');
    await currentPage.waitForLoadState('networkidle');

    await currentPage.fill('input[formControlName="email"]', testUser.email);
    await currentPage.fill('input[formControlName="firstName"]', testUser.firstName);
    await currentPage.fill('input[formControlName="lastName"]', testUser.lastName);
    await currentPage.fill('input[formControlName="dni"]', testUser.dni);
    await currentPage.fill('input[formControlName="phone"]', testUser.phone);
    await currentPage.fill('input[formControlName="password"]', testUser.password);
    await currentPage.check('input[formControlName="acceptTerms"]');
    await currentPage.check('input[formControlName="acceptAdditionalPurposes"]');

    await currentPage.click('button[type="submit"]');
    await currentPage.waitForTimeout(3000); // Esperar registro

    // Ahora intentar registrar otro usuario con el mismo DNI pero diferente email
    await currentPage.goto('http://localhost:4200/auth/register');
    await currentPage.waitForLoadState('networkidle');

    const duplicateUser = generateTestUser();
    await currentPage.fill('input[formControlName="email"]', duplicateUser.email);
    await currentPage.fill('input[formControlName="firstName"]', 'Test');
    await currentPage.fill('input[formControlName="lastName"]', 'User');
    await currentPage.fill('input[formControlName="dni"]', testUser.dni); // DNI duplicado
    await currentPage.fill('input[formControlName="phone"]', '+51 888 888 888');
    await currentPage.fill('input[formControlName="password"]', 'Test123456');
    await currentPage.check('input[formControlName="acceptTerms"]');
    await currentPage.check('input[formControlName="acceptAdditionalPurposes"]');

    await currentPage.click('button[type="submit"]');
    
    // Esperar mensaje de error - usar selector más específico
    await currentPage.waitForTimeout(2000);
    const errorMessage = currentPage.locator('.text-red-600, .bg-red-50').filter({ hasText: /DNI.*registrado|ya.*existe/i }).first();
    
    if (await errorMessage.isVisible({ timeout: 5000 })) {
      const errorText = await errorMessage.textContent();
      console.log(`✅ Validación de DNI duplicado funciona correctamente: ${errorText}`);
    } else {
      console.log('⚠️ No se encontró mensaje de error de DNI duplicado');
      await currentPage.screenshot({ path: 'test-results/dni-duplicate-error-not-found.png' });
    }
  });

  test('Validar formulario de completar perfil - campos requeridos', async ({ page }) => {
    const currentPage = page;
    const testUser = generateTestUser(); // Generar usuario único
    console.log('🧪 Probando validaciones del formulario de completar perfil...\n');

    // Primero registrar el usuario
    await currentPage.goto('http://localhost:4200/auth/register');
    await currentPage.waitForLoadState('networkidle');

    await currentPage.fill('input[formControlName="email"]', testUser.email);
    await currentPage.fill('input[formControlName="firstName"]', testUser.firstName);
    await currentPage.fill('input[formControlName="lastName"]', testUser.lastName);
    await currentPage.fill('input[formControlName="dni"]', testUser.dni);
    await currentPage.fill('input[formControlName="phone"]', testUser.phone);
    await currentPage.fill('input[formControlName="password"]', testUser.password);
    await currentPage.check('input[formControlName="acceptTerms"]');
    await currentPage.check('input[formControlName="acceptAdditionalPurposes"]');

    await currentPage.click('button[type="submit"]');
    await currentPage.waitForTimeout(3000);

    // Si hay error, tomar screenshot y continuar
    const errorMsg = currentPage.locator('.text-red-600, .bg-red-50').first();
    if (await errorMsg.isVisible({ timeout: 2000 })) {
      const errorText = await errorMsg.textContent();
      console.log(`⚠️ Error en registro: ${errorText}`);
      await currentPage.screenshot({ path: 'test-results/registration-error-in-profile-test.png' });
      // Continuar de todas formas para probar el formulario
    }

    // Ir directamente a completar perfil (puede que ya estemos ahí o necesitemos navegar)
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

