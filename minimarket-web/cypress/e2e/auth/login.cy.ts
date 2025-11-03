import { LoginPage } from '../../support/page-objects/login.page';

describe('Login', () => {
  const loginPage = new LoginPage();

  beforeEach(() => {
    loginPage.visit();
  });

  it('should display login form', () => {
    loginPage.verifyFormVisible();
  });

  it('should login successfully with valid credentials', () => {
    cy.get('[data-cy=email-input]').type('admin');
    cy.get('[data-cy=password-input]').type('Admin@1234');
    cy.get('[data-cy=login-button]').click();

    // Verificar redirección
    cy.url().should('not.include', '/login');
    cy.get('[data-cy=user-menu]').should('be.visible');
  });

  it('should show error with invalid credentials', () => {
    cy.get('[data-cy=email-input]').type('invalid@email.com');
    cy.get('[data-cy=password-input]').type('WrongPassword123');
    cy.get('[data-cy=login-button]').click();

    cy.get('[data-cy=error-message]', { timeout: 5000 }).should('be.visible');
    cy.url().should('include', '/login');
  });

  it('should validate required fields', () => {
    cy.get('[data-cy=login-button]').click();

    cy.get('[data-cy=email-error]').should('contain', 'El usuario es requerido');
    cy.get('[data-cy=password-error]').should('contain', 'La contraseña es requerida');
  });

  it('should toggle password visibility', () => {
    cy.get('[data-cy=password-input]').should('have.attr', 'type', 'password');
    cy.get('[data-cy=toggle-password]').click();
    cy.get('[data-cy=password-input]').should('have.attr', 'type', 'text');
  });

  it('should navigate to forgot password page', () => {
    cy.get('[data-cy=forgot-password-link]').click();
    // Verificar que existe el enlace (aunque la página aún no esté implementada)
    cy.get('[data-cy=forgot-password-link]').should('exist');
  });
});

