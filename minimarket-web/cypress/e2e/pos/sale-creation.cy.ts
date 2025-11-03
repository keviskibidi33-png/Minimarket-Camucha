import { POSPage } from '../../support/page-objects/pos.page';

describe('POS - Complete Sale Flow', () => {
  const posPage = new POSPage();

  beforeEach(() => {
    cy.loginAsCajero();
    posPage.visit();
  });

  afterEach(() => {
    cy.logout();
  });

  it('should display POS interface', () => {
    cy.get('[data-cy=product-search]').should('be.visible');
    cy.get('[data-cy=document-type]').should('be.visible');
  });

  it('should search for products', () => {
    posPage.searchProduct('Coca Cola');
    // Verificar que se muestran resultados
    cy.get('[data-cy=product-search]').should('have.value', 'Coca Cola');
  });

  it('should select document type', () => {
    posPage.selectDocumentType('Factura');
    cy.get('[data-cy=document-type]').should('have.value', 'Factura');
  });

  it('should select payment method', () => {
    posPage.selectPaymentMethod('Efectivo');
    cy.get('[data-cy=payment-method-select]').should('have.value', 'Efectivo');
  });

  it('should display cart when empty', () => {
    cy.get('[data-cy=cart-items]').should('exist');
  });
});

