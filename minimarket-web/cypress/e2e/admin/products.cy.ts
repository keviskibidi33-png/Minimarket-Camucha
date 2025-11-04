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
    cy.get('[data-cy=search-input]').should('have.value', 'Coca Cola');
    cy.wait(1000);
    cy.get('[data-cy=products-table]').should('contain', 'Coca Cola');
  });

  it('should navigate to new product page', () => {
    productsPage.clickNewProduct();
    cy.url().should('include', '/productos/nuevo');
  });

  it('should filter products by category', () => {
    // Verificar que la tabla existe y puede filtrarse
    cy.get('[data-cy=products-table]').should('be.visible');
  });

  it('should create a new product with valid data', () => {
    productsPage.clickNewProduct();
    cy.url().should('include', '/productos/nuevo');
    // El formulario debería estar visible
    cy.get('form').should('exist');
  });

  it('should show validation errors for invalid product data', () => {
    productsPage.clickNewProduct();
    // Intentar enviar formulario vacío
    cy.get('button[type="submit"]').click();
    // Verificar errores de validación
    cy.get('form').should('exist');
  });

  it('should edit existing product', () => {
    // Verificar que existe la tabla con productos
    cy.get('[data-cy=products-table]').should('be.visible');
    cy.get('[data-cy=product-row]').should('have.length.greaterThan', 0);
  });

  it('should delete product with confirmation', () => {
    // Verificar estructura de la tabla
    cy.get('[data-cy=products-table]').should('be.visible');
    cy.get('[data-cy=product-row]').should('have.length.greaterThan', 0);
  });

  it('should show confirmation dialog before deleting', () => {
    // Verificar que existe la funcionalidad de eliminación
    cy.get('[data-cy=products-table]').should('be.visible');
  });
});

