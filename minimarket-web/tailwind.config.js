/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // Colores para TIENDA (Usuario)
        primary: {
          DEFAULT: '#4CAF50', // Verde principal para tienda
          light: '#6BC66F',
          dark: '#3A8E3D',
        },
        secondary: {
          DEFAULT: '#FF9800', // Naranja para ofertas
          light: '#FFB74D',
          dark: '#F57C00',
          800: '#E65100', // Para texto sobre secondary
        },
        // Colores para ADMIN PANEL
        'primary-admin': {
          DEFAULT: '#2563EB', // Azul principal admin
          dark: '#2A629A', // Azul para login y algunos componentes
        },
        // Backgrounds
        'background-light': '#F5F5F5',
        'background-dark': '#101922',
        // Text colors
        'text-light': '#333333',
        'text-dark': '#F5F7F8',
        'text-primary-light': '#212529',
        'text-primary-dark': '#F8F9FA',
        'text-secondary-light': '#6c757d',
        'text-secondary-dark': '#adb5bd',
        // Card colors
        'card-light': '#FFFFFF',
        'card-dark': '#1c2a38', // También #2c2c2c en algunos casos
        // Border colors
        'border-light': '#e0e0e0', // También #cccccc, #dee2e6 según referencia
        'border-dark': '#4a5568', // También #424242, #495057 según referencia
        // Placeholder colors
        'placeholder-light': '#888888',
        'placeholder-dark': '#a0aec0',
        // Accent colors
        accent: '#FFC107',
        // Subtle colors (admin)
        subtle: {
          light: '#F3F4F6',
          dark: '#374151',
        },
        'subtle-light': '#F3F4F6',
        'subtle-dark': '#374151',
        // Success, danger, warning, info
        success: '#10B981',
        danger: '#EF4444',
        warning: '#F59E0B',
        info: '#3B82F6',
      },
      fontFamily: {
        display: ['Inter', 'sans-serif'],
      },
      borderRadius: {
        DEFAULT: '0.25rem',
        lg: '0.5rem',
        xl: '0.75rem',
        '2xl': '1rem',
        full: '9999px',
      },
    },
  },
  plugins: [],
}

