import { defineConfig, devices } from '@playwright/test';

/**
 * Configuración de Playwright para tests E2E
 * @see https://playwright.dev/docs/test-configuration
 */
export default defineConfig({
  testDir: './e2e',
  /* Ejecutar tests en paralelo */
  fullyParallel: true,
  /* Fallar el build si hay tests rotos */
  forbidOnly: !!process.env.CI,
  /* Reintentar en CI si falla */
  retries: process.env.CI ? 2 : 0,
  /* Opciones para workers */
  workers: process.env.CI ? 1 : undefined,
  /* Configuración del reporter */
  reporter: 'html',
  /* Configuración compartida para todos los proyectos */
  use: {
    /* URL base para usar en navegación */
    baseURL: 'http://localhost:4200',
    /* Recopilar trace cuando se repite un test fallido */
    trace: 'on-first-retry',
    /* Screenshot en fallos */
    screenshot: 'only-on-failure',
  },

  /* Configurar proyectos para diferentes navegadores */
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ],

  /* Servidor de desarrollo local */
  webServer: {
    command: 'npm run start',
    url: 'http://localhost:4200',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
});

