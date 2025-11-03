// Helper functions for Cypress tests

export const formatCurrency = (amount: number): string => {
  return `S/ ${amount.toFixed(2)}`;
};

export const parseCurrency = (text: string): number => {
  return parseFloat(text.replace('S/ ', '').replace(',', ''));
};

export const waitForAPI = (alias: string) => {
  cy.wait(`@${alias}`, { timeout: 10000 });
};

export const generateProductCode = (): string => {
  return `TEST${Date.now()}`;
};

export const generateCustomerDocument = (): string => {
  return `${Math.floor(10000000000 + Math.random() * 90000000000)}`;
};

