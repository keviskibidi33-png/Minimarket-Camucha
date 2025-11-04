import { defineConfig } from 'cypress';

export default defineConfig({
  e2e: {
    baseUrl: 'http://localhost:4200',
    viewportWidth: 1920,
    viewportHeight: 1080,
    video: true,
    screenshotOnRunFailure: true,
    defaultCommandTimeout: 10000,
    requestTimeout: 10000,
    responseTimeout: 10000,
    retries: {
      runMode: 2,
      openMode: 0
    },
    env: {
      apiUrl: 'http://localhost:5000/api',
      testUser: {
        email: 'admin', // Usuario admin (según DatabaseSeeder.cs)
        password: 'Admin123!' // Contraseña admin (según DatabaseSeeder.cs)
      },
      testCajero: {
        email: 'cajero', // Usuario cajero (según DatabaseSeeder.cs)
        password: 'Cajero123!' // Contraseña cajero (según DatabaseSeeder.cs)
      }
    },
    setupNodeEvents(on, config) {
      // Plugin setup aquí
      return config;
    }
  }
});

