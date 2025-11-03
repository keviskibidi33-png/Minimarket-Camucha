import { DashboardPage } from '../../support/page-objects/dashboard.page';

describe('Dashboard and Reports', () => {
  const dashboardPage = new DashboardPage();

  beforeEach(() => {
    cy.loginAsAdmin();
    dashboardPage.visit();
  });

  afterEach(() => {
    cy.logout();
  });

  it('should display main KPIs', () => {
    cy.get('[data-cy=total-sales]').should('be.visible');
    cy.get('[data-cy=total-profit]').should('be.visible');
    cy.get('[data-cy=transactions-count]').should('be.visible');
    cy.get('[data-cy=inventory-value]').should('be.visible');
  });

  it('should display sales trend chart', () => {
    cy.get('[data-cy=sales-chart]').should('be.visible');
  });
});

