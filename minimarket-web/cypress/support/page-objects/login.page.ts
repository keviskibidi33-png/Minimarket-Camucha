export class LoginPage {
  visit() {
    cy.visit('/login');
  }

  getEmailInput() {
    return cy.get('[data-cy=email-input]');
  }

  getPasswordInput() {
    return cy.get('[data-cy=password-input]');
  }

  getLoginButton() {
    return cy.get('[data-cy=login-button]');
  }

  getErrorMessage() {
    return cy.get('[data-cy=error-message]');
  }

  getTogglePasswordButton() {
    return cy.get('[data-cy=toggle-password]');
  }

  getForgotPasswordLink() {
    return cy.get('[data-cy=forgot-password-link]');
  }

  login(email: string, password: string) {
    this.getEmailInput().type(email);
    this.getPasswordInput().type(password);
    this.getLoginButton().click();
  }

  verifyFormVisible() {
    this.getEmailInput().should('be.visible');
    this.getPasswordInput().should('be.visible');
    this.getLoginButton().should('be.visible');
  }

  verifyErrorMessage(message: string) {
    this.getErrorMessage().should('be.visible').and('contain', message);
  }

  togglePasswordVisibility() {
    this.getTogglePasswordButton().click();
  }

  verifyPasswordInputType(type: 'password' | 'text') {
    this.getPasswordInput().should('have.attr', 'type', type);
  }
}

