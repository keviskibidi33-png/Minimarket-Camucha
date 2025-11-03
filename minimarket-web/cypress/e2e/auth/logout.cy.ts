describe('Logout', () => {
  beforeEach(() => {
    cy.loginAsAdmin();
  });

  it('should logout successfully', () => {
    cy.get('[data-cy=logout-button]').click();
    cy.url().should('include', '/login');
    cy.get('[data-cy=email-input]').should('be.visible');
  });
});

