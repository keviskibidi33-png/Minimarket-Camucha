/// <reference types="cypress" />

declare global {
  namespace Cypress {
    interface Chainable {
      login(email: string, password: string): Chainable<void>;
      loginAsAdmin(): Chainable<void>;
      loginAsCajero(): Chainable<void>;
      logout(): Chainable<void>;
      createProduct(product: any): Chainable<void>;
      searchProduct(query: string): Chainable<void>;
      addProductToCart(productName: string, quantity: number): Chainable<void>;
      completeSale(paymentMethod: string, amountPaid: number): Chainable<void>;
    }
  }
}

// Login command
Cypress.Commands.add('login', (email: string, password: string) => {
  cy.visit('/login');
  cy.get('[data-cy=email-input]').type(email);
  cy.get('[data-cy=password-input]').type(password);
  cy.get('[data-cy=login-button]').click();
  cy.url().should('not.include', '/login');
  // Wait for navigation and verify user menu is visible
  cy.get('[data-cy=user-menu]', { timeout: 10000 }).should('be.visible');
});

Cypress.Commands.add('loginAsAdmin', () => {
  cy.login(
    Cypress.env('testUser').email,
    Cypress.env('testUser').password
  );
});

Cypress.Commands.add('loginAsCajero', () => {
  cy.login(
    Cypress.env('testCajero').email,
    Cypress.env('testCajero').password
  );
});

Cypress.Commands.add('logout', () => {
  cy.get('[data-cy=user-menu]').click();
  cy.get('[data-cy=logout-button]').click();
  cy.url().should('include', '/login');
});

Cypress.Commands.add('createProduct', (product) => {
  cy.get('[data-cy=new-product-button]').click();
  cy.get('[data-cy=product-code]').type(product.code);
  cy.get('[data-cy=product-name]').type(product.name);
  cy.get('[data-cy=purchase-price]').type(product.purchasePrice.toString());
  cy.get('[data-cy=sale-price]').type(product.salePrice.toString());
  cy.get('[data-cy=stock]').type(product.stock.toString());
  cy.get('[data-cy=category-select]').click();
  cy.get(`[data-cy=category-option-${product.categoryId}]`).click();
  cy.get('[data-cy=save-product-button]').click();
  cy.get('[data-cy=success-toast]').should('be.visible');
});

Cypress.Commands.add('searchProduct', (query: string) => {
  cy.get('[data-cy=product-search]').clear().type(query);
  cy.get('[data-cy=search-results]').should('be.visible');
});

Cypress.Commands.add('addProductToCart', (productName: string, quantity: number) => {
  cy.searchProduct(productName);
  cy.get(`[data-cy=product-card-${productName}]`).within(() => {
    cy.get('[data-cy=quantity-input]').clear().type(quantity.toString());
    cy.get('[data-cy=add-to-cart]').click();
  });
  cy.get('[data-cy=cart-count]').should('contain', quantity);
});

Cypress.Commands.add('completeSale', (paymentMethod: string, amountPaid: number) => {
  cy.get('[data-cy=payment-method-select]').click();
  cy.get(`[data-cy=payment-${paymentMethod}]`).click();
  cy.get('[data-cy=amount-paid]').type(amountPaid.toString());
  cy.get('[data-cy=complete-sale-button]').click();
  cy.get('[data-cy=sale-success-dialog]').should('be.visible');
});

export {};

