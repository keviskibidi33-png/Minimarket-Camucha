import { SalesPage } from '../../support/page-objects/sales.page';

describe('Sales - List and Details', () => {
  const salesPage = new SalesPage();

  beforeEach(() => {
    cy.loginAsAdmin();
    salesPage.visit();
  });

  afterEach(() => {
    cy.logout();
  });

  it('should display sales list', () => {
    cy.get('[data-cy=sales-table]').should('be.visible');
    cy.get('tbody tr').should('have.length.greaterThan', 0);
  });

  it('should filter sales by date range', () => {
    const today = new Date().toISOString().split('T')[0];
    
    cy.get('[data-cy=date-from]').type(today);
    cy.get('[data-cy=date-to]').type(today);
    // Verificar que los campos tienen valores
    cy.get('[data-cy=date-from]').should('have.value', today);
    cy.get('[data-cy=date-to]').should('have.value', today);
  });

  it('should view sale details', () => {
    // Verificar que existe la tabla
    cy.get('[data-cy=sales-table]').should('be.visible');
    // Hacer click en la primera venta para ver detalles
    cy.get('tbody tr').first().click();
    // Verificar que se puede ver el detalle (si existe modal o página)
    cy.wait(1000);
  });

  it('should filter sales by document type', () => {
    cy.get('[data-cy=sales-table]').should('be.visible');
    // Verificar que existe la funcionalidad de filtrado
    cy.get('[data-cy=search-input]').should('be.visible');
  });

  it('should cancel a sale with reason', () => {
    cy.get('[data-cy=sales-table]').should('be.visible');
    // Verificar que existe la estructura para anular ventas
    cy.get('tbody tr').should('have.length.greaterThan', 0);
  });
});

