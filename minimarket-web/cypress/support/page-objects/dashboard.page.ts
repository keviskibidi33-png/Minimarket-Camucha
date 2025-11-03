export class DashboardPage {
  visit() {
    cy.visit('/admin/dashboard');
  }

  verifyKPIsVisible() {
    cy.get('[data-cy=total-sales]').should('be.visible');
    cy.get('[data-cy=total-profit]').should('be.visible');
    cy.get('[data-cy=transactions-count]').should('be.visible');
    cy.get('[data-cy=inventory-value]').should('be.visible');
  }

  verifySalesChartVisible() {
    cy.get('[data-cy=sales-chart]').should('be.visible');
    cy.get('canvas').should('exist');
  }

  verifyTopProductsVisible() {
    cy.get('[data-cy=top-products]').should('be.visible');
    cy.get('[data-cy=product-item]').should('have.length.greaterThan', 0);
  }

  verifyLowStockAlertsVisible() {
    cy.get('[data-cy=low-stock-alerts]').should('be.visible');
  }

  getLowStockProducts() {
    return cy.get('[data-cy=low-stock-product]');
  }

  filterByDateRange(range: string) {
    cy.get('[data-cy=date-filter]').select(range);
  }

  navigateToReports() {
    cy.get('[data-cy=reports-menu]').click();
    cy.get('[data-cy=sales-report]').click();
  }

  generateSalesReport(startDate: string, endDate: string) {
    cy.get('[data-cy=report-date-from]').type(startDate);
    cy.get('[data-cy=report-date-to]').type(endDate);
    cy.get('[data-cy=generate-report]').click();
  }

  verifyReportTableVisible() {
    cy.get('[data-cy=report-table]').should('be.visible');
    cy.get('[data-cy=export-pdf]').should('be.visible');
  }

  verifyComparisonWidgetVisible() {
    cy.get('[data-cy=comparison-widget]').should('be.visible');
    cy.get('[data-cy=current-month-sales]').should('exist');
    cy.get('[data-cy=previous-month-sales]').should('exist');
    cy.get('[data-cy=percentage-change]').should('match', /[\+\-]\d+\.\d+%/);
  }
}

