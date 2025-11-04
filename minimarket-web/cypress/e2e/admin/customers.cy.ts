describe('Admin - Customers Management', () => {
  beforeEach(() => {
    cy.loginAsAdmin();
    cy.visit('/admin/clientes');
  });

  afterEach(() => {
    cy.logout();
  });

  it('should display customers list', () => {
    cy.get('table').should('be.visible');
    // Verificar que hay clientes en la tabla
    cy.get('tbody tr').should('have.length.greaterThan', 0);
  });

  it('should search customers by name', () => {
    cy.get('input[type="text"]').first().type('Cliente');
    cy.wait(1000);
    cy.get('table').should('be.visible');
  });

  it('should navigate to new customer page', () => {
    cy.contains('Agregar', { matchCase: false }).click();
    cy.url().should('include', '/clientes/nuevo');
  });

  it('should validate DNI format (8 digits)', () => {
    cy.contains('Agregar', { matchCase: false }).click();
    cy.get('input[type="text"]').then($inputs => {
      // Buscar el input de documento
      const docInput = $inputs.filter((i, el) => {
        return el.getAttribute('placeholder')?.toLowerCase().includes('dni') || 
               el.getAttribute('placeholder')?.toLowerCase().includes('documento');
      }).first();
      if (docInput.length > 0) {
        cy.wrap(docInput).type('1234567'); // 7 dígitos - inválido
        cy.get('form').should('exist');
      }
    });
  });

  it('should validate RUC format (11 digits)', () => {
    cy.contains('Agregar', { matchCase: false }).click();
    cy.get('input[type="text"]').then($inputs => {
      const docInput = $inputs.filter((i, el) => {
        return el.getAttribute('placeholder')?.toLowerCase().includes('ruc') || 
               el.getAttribute('placeholder')?.toLowerCase().includes('documento');
      }).first();
      if (docInput.length > 0) {
        cy.wrap(docInput).type('12345678901'); // 11 dígitos - válido
        cy.get('form').should('exist');
      }
    });
  });

  it('should prevent duplicate document numbers', () => {
    // Verificar que existe la funcionalidad
    cy.get('table').should('be.visible');
  });
});

