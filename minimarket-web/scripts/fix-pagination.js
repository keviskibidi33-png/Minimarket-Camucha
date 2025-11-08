#!/usr/bin/env node

/**
 * Script de corrección automática para problemas de paginación
 * Reemplaza código problemático con el componente de paginación
 */

const fs = require('fs');
const path = require('path');

const profileComponentPath = path.join(__dirname, '../src/app/features/store/profile/profile.component.html');

if (!fs.existsSync(profileComponentPath)) {
  console.error('❌ No se encontró el archivo profile.component.html');
  process.exit(1);
}

let content = fs.readFileSync(profileComponentPath, 'utf8');

// Buscar y reemplazar la sección de paginación problemática
const paginationPattern = /<!-- Paginación -->[\s\S]*?<\/div>\s*<\/div>\s*<\/div>/;

if (paginationPattern.test(content)) {
  const replacement = `<!-- Paginación -->
                <app-pagination
                  [currentPage]="currentPage()"
                  [totalItems]="filteredOrders().length"
                  [itemsPerPage]="itemsPerPage"
                  (pageChange)="currentPage.set($event)">
                </app-pagination>`;
  
  content = content.replace(paginationPattern, replacement);
  fs.writeFileSync(profileComponentPath, content, 'utf8');
  console.log('✅ Paginación reemplazada con componente <app-pagination>');
} else {
  console.log('ℹ️  No se encontró código de paginación para reemplazar');
}

