export class ProductsPage {
  visit() {
    cy.visit('/admin/productos');
  }

  clickNewProduct() {
    cy.get('[data-cy=new-product-button]').click();
  }

  fillProductForm(product: any) {
    cy.get('[data-cy=product-code]').type(product.code);
    cy.get('[data-cy=product-name]').type(product.name);
    cy.get('[data-cy=purchase-price]').type(product.purchasePrice);
    cy.get('[data-cy=sale-price]').type(product.salePrice);
    cy.get('[data-cy=stock]').type(product.stock);
    cy.get('[data-cy=category-select]').select(product.category);
  }

  submitForm() {
    cy.get('[data-cy=submit-button]').click();
  }

  verifyProductInList(productName: string) {
    cy.get('[data-cy=products-table]').should('contain', productName);
  }

  searchProduct(query: string) {
    cy.get('[data-cy=search-input]').type(query);
  }

  editProduct(productName: string) {
    cy.get(`[data-cy=edit-${productName}]`).click();
  }

  deleteProduct(productName: string) {
    cy.get(`[data-cy=delete-${productName}]`).click();
    cy.get('[data-cy=confirm-delete]').click();
  }

  verifyProductsTableVisible() {
    cy.get('[data-cy=products-table]').should('be.visible');
  }

  getProductRows() {
    return cy.get('[data-cy=product-row]');
  }

  filterByCategory(category: string) {
    cy.get('[data-cy=category-filter]').select(category);
  }

  toggleLowStockFilter() {
    cy.get('[data-cy=low-stock-filter]').click();
  }

  exportToExcel() {
    cy.get('[data-cy=export-excel-button]').click();
  }

  navigateToPage(pageNumber: number) {
    cy.get(`[data-cy=page-${pageNumber}]`).click();
  }

  verifyErrorMessages() {
    cy.get('[data-cy=code-error]').should('contain', 'Código es requerido');
    cy.get('[data-cy=name-error]').should('contain', 'Nombre es requerido');
    cy.get('[data-cy=price-error]').should('contain', 'Precio es requerido');
  }
}

