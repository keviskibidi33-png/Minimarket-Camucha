export class POSPage {
  visit() {
    cy.visit('/pos');
  }

  searchProduct(query: string) {
    cy.get('[data-cy=product-search]').type(query);
  }

  selectProduct(productName: string) {
    cy.get(`[data-cy=product-${productName}]`).click();
  }

  setQuantity(quantity: number) {
    cy.get('[data-cy=quantity-input]').clear().type(quantity.toString());
  }

  addToCart() {
    cy.get('[data-cy=add-to-cart-button]').click();
  }

  selectPaymentMethod(method: string) {
    cy.get('[data-cy=payment-method]').select(method);
  }

  enterAmountPaid(amount: number) {
    cy.get('[data-cy=amount-paid]').type(amount.toString());
  }

  completeSale() {
    cy.get('[data-cy=complete-sale]').click();
  }

  verifySaleCreated() {
    cy.get('[data-cy=sale-success]').should('be.visible');
  }

  verifyInvoiceDisplayed() {
    cy.get('[data-cy=invoice-preview]').should('be.visible');
  }

  selectDocumentType(type: 'Boleta' | 'Factura') {
    cy.get('[data-cy=document-type]').select(type);
  }

  searchCustomer(query: string) {
    cy.get('[data-cy=customer-search]').type(query);
  }

  selectCustomer(customerIndex: number) {
    cy.get(`[data-cy=customer-option-${customerIndex}]`).click();
  }

  verifyCustomerSelected(customerName: string) {
    cy.get('[data-cy=selected-customer]').should('contain', customerName);
  }

  getCartItems() {
    return cy.get('[data-cy=cart-items]');
  }

  getSubtotal() {
    return cy.get('[data-cy=subtotal]');
  }

  getIGV() {
    return cy.get('[data-cy=igv]');
  }

  getTotal() {
    return cy.get('[data-cy=total]');
  }

  getChangeAmount() {
    return cy.get('[data-cy=change-amount]');
  }

  removeCartItem(itemIndex: number) {
    cy.get(`[data-cy=remove-item-${itemIndex}]`).click();
  }

  clearCart() {
    cy.get('[data-cy=clear-cart]').click();
    cy.get('[data-cy=confirm-clear]').click();
  }

  applyDiscount(percentage: number) {
    cy.get('[data-cy=apply-discount]').click();
    cy.get('[data-cy=discount-percentage]').type(percentage.toString());
    cy.get('[data-cy=apply-discount-button]').click();
  }

  getInvoiceNumber() {
    return cy.get('[data-cy=invoice-number]');
  }

  printInvoice() {
    cy.get('[data-cy=print-invoice]').click();
  }

  startNewSale() {
    cy.get('[data-cy=new-sale]').click();
  }
}

