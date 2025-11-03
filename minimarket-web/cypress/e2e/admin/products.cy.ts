import { ProductsPage } from '../../support/page-objects/products.page';

describe('Admin - Products Management', () => {
  const productsPage = new ProductsPage();

  beforeEach(() => {
    cy.loginAsAdmin();
    productsPage.visit();
  });

  afterEach(() => {
    cy.logout();
  });

  it('should display products list', () => {
    cy.get('[data-cy=products-table]').should('be.visible');
    cy.get('[data-cy=product-row]').should('have.length.greaterThan', 0);
  });

  it('should search products by name', () => {
    productsPage.searchProduct('Coca Cola');

    cy.get('[data-cy=product-row]').should('have.length.greaterThan', 0);
    cy.get('[data-cy=products-table]').should('contain', 'Coca Cola');
  });

  it('should navigate to new product page', () => {
    productsPage.clickNewProduct();
    cy.url().should('include', '/productos/nuevo');
  });
});

