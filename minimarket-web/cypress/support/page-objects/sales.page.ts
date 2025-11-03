export class SalesPage {
  visit() {
    cy.visit('/admin/ventas');
  }

  verifySalesTableVisible() {
    cy.get('[data-cy=sales-table]').should('be.visible');
  }

  getSaleRows() {
    return cy.get('[data-cy=sale-row]');
  }

  filterByDateRange(startDate: string, endDate: string) {
    cy.get('[data-cy=date-from]').type(startDate);
    cy.get('[data-cy=date-to]').type(endDate);
    cy.get('[data-cy=apply-filter]').click();
  }

  filterByDocumentType(type: string) {
    cy.get('[data-cy=document-type-filter]').select(type);
    cy.get('[data-cy=apply-filter]').click();
  }

  filterByStatus(status: string) {
    cy.get('[data-cy=status-filter]').select(status);
    cy.get('[data-cy=apply-filter]').click();
  }

  viewSaleDetails(saleIndex: number = 0) {
    cy.get('[data-cy=sale-row]').eq(saleIndex).click();
  }

  verifySaleDetailModalVisible() {
    cy.get('[data-cy=sale-detail-modal]').should('be.visible');
    cy.get('[data-cy=sale-products]').should('exist');
    cy.get('[data-cy=sale-total]').should('be.visible');
    cy.get('[data-cy=sale-payment-method]').should('be.visible');
  }

  reprintInvoice(saleIndex: number = 0) {
    cy.get('[data-cy=sale-row]').eq(saleIndex).within(() => {
      cy.get('[data-cy=reprint-button]').click();
    });
  }

  verifyInvoicePreviewVisible() {
    cy.get('[data-cy=invoice-preview]').should('be.visible');
    cy.get('[data-cy=print-button]').should('be.visible');
  }

  cancelSale(saleIndex: number = 0, reason: string) {
    cy.get('[data-cy=sale-row]').eq(saleIndex).within(() => {
      cy.get('[data-cy=cancel-button]').click();
    });
    cy.get('[data-cy=cancellation-reason]').type(reason);
    cy.get('[data-cy=confirm-cancellation]').click();
  }

  searchByDocumentNumber(documentNumber: string) {
    cy.get('[data-cy=search-input]').type(documentNumber);
    cy.get('[data-cy=search-button]').click();
  }

  exportToExcel() {
    cy.get('[data-cy=export-excel]').click();
  }

  verifySaleStatus(saleIndex: number, status: string) {
    cy.get('[data-cy=sale-row]').eq(saleIndex).within(() => {
      cy.get('[data-cy=status-badge]').should('contain', status);
    });
  }

  verifyCancelButtonDisabled(saleIndex: number) {
    cy.get('[data-cy=sale-row]').eq(saleIndex).within(() => {
      cy.get('[data-cy=cancel-button]').should('be.disabled');
    });
  }
}

