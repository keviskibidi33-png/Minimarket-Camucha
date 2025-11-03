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
  });

  it('should filter sales by date range', () => {
    const today = new Date().toISOString().split('T')[0];
    
    cy.get('[data-cy=date-from]').type(today);
    cy.get('[data-cy=date-to]').type(today);
    cy.get('[data-cy=apply-filter]').click();

    // Verificar que se aplicó el filtro
    cy.url().should('include', 'date');
  });
});

