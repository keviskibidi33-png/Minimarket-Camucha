describe('Mobile Responsive Tests', () => {
  const mobileViewport = { width: 375, height: 667 }; // iPhone SE

  beforeEach(() => {
    cy.viewport(mobileViewport.width, mobileViewport.height);
  });

  it('should display login form on mobile', () => {
    cy.visit('/login');
    cy.get('[data-cy=email-input]').should('be.visible');
    cy.get('[data-cy=password-input]').should('be.visible');
    cy.get('[data-cy=login-button]').should('be.visible');
  });

  it('should allow login on mobile', () => {
    cy.visit('/login');
    cy.get('[data-cy=email-input]').type('admin');
    cy.get('[data-cy=password-input]').type('Admin@1234');
    cy.get('[data-cy=login-button]').click();
    
    cy.url().should('not.include', '/login');
  });
});

