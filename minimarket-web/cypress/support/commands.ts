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
  cy.get('[data-cy=category-select]').select(product.categoryId.toString());
  cy.get('[data-cy=submit-button]').click();
  // Esperar a que se complete la operación
  cy.url().should('include', '/productos');
});

Cypress.Commands.add('searchProduct', (query: string) => {
  cy.get('[data-cy=product-search]').clear().type(query);
  cy.wait(500); // Esperar a que se procese la búsqueda
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
  cy.get('[data-cy=payment-method-select]').select(paymentMethod);
  if (paymentMethod === 'Efectivo') {
    cy.get('[data-cy=amount-paid]').clear().type(amountPaid.toString());
  }
  cy.get('[data-cy=complete-sale-button]').click();
  // Esperar a que se procese la venta
  cy.wait(2000);
});

export {};

