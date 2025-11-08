#!/usr/bin/env node

/**
 * Script de validación de templates Angular
 * Verifica problemas comunes en templates HTML
 */

const fs = require('fs');
const path = require('path');

const issues = [];

function validateTemplate(filePath) {
  const content = fs.readFileSync(filePath, 'utf8');
  const lines = content.split('\n');
  
  // Verificar tags no cerrados
  const openTags = [];
  let inForBlock = false;
  let forBlockStart = -1;
  
  lines.forEach((line, index) => {
    const lineNum = index + 1;
    
    // Detectar inicio de @for
    if (line.includes('@for')) {
      inForBlock = true;
      forBlockStart = lineNum;
    }
    
    // Detectar fin de @for
    if (line.trim() === '}' && inForBlock) {
      inForBlock = false;
    }
    
    // Verificar tags HTML
    const tagMatches = line.matchAll(/<(\w+)([^>]*)>/g);
    for (const match of tagMatches) {
      const tagName = match[1];
      const attributes = match[2];
      
      // Ignorar tags auto-cerrados
      if (attributes.trim().endsWith('/') || ['input', 'img', 'br', 'hr', 'meta', 'link'].includes(tagName.toLowerCase())) {
        continue;
      }
      
      openTags.push({ tag: tagName, line: lineNum });
    }
    
    // Verificar tags de cierre
    const closeMatches = line.matchAll(/<\/(\w+)>/g);
    for (const match of closeMatches) {
      const tagName = match[1];
      const lastOpen = openTags.pop();
      
      if (!lastOpen || lastOpen.tag !== tagName) {
        issues.push({
          file: filePath,
          line: lineNum,
          type: 'MISMATCHED_TAG',
          message: `Tag de cierre </${tagName}> no coincide con tag abierto`
        });
      }
    }
    
    // Verificar @for con botones
    if (inForBlock && line.includes('<button') && !line.includes('</button>')) {
      // Verificar si el botón está en múltiples líneas
      const nextLines = lines.slice(index, Math.min(index + 10, lines.length));
      const buttonContent = nextLines.join('\n');
      if (!buttonContent.includes('</button>')) {
        issues.push({
          file: filePath,
          line: lineNum,
          type: 'UNCLOSED_BUTTON_IN_FOR',
          message: 'Botón dentro de @for puede tener problemas de parsing. Considera usar un componente separado.'
        });
      }
    }
  });
  
  // Verificar tags no cerrados
  if (openTags.length > 0) {
    openTags.forEach(tag => {
      issues.push({
        file: filePath,
        line: tag.line,
        type: 'UNCLOSED_TAG',
        message: `Tag <${tag.tag}> no fue cerrado`
      });
    });
  }
}

function findTemplates(dir) {
  const files = [];
  
  function walk(currentPath) {
    const entries = fs.readdirSync(currentPath, { withFileTypes: true });
    
    for (const entry of entries) {
      const fullPath = path.join(currentPath, entry.name);
      
      if (entry.isDirectory() && !entry.name.startsWith('.') && entry.name !== 'node_modules') {
        walk(fullPath);
      } else if (entry.isFile() && entry.name.endsWith('.component.html')) {
        files.push(fullPath);
      }
    }
  }
  
  walk(dir);
  return files;
}

// Ejecutar validación
const srcDir = path.join(__dirname, '../src');
const templates = findTemplates(srcDir);

console.log(`Validando ${templates.length} templates...\n`);

templates.forEach(template => {
  validateTemplate(template);
});

// Reportar resultados
if (issues.length === 0) {
  console.log('✅ Todos los templates están correctos');
  process.exit(0);
} else {
  console.log(`❌ Se encontraron ${issues.length} problemas:\n`);
  
  issues.forEach(issue => {
    console.log(`📄 ${path.relative(srcDir, issue.file)}:${issue.line}`);
    console.log(`   ${issue.type}: ${issue.message}\n`);
  });
  
  process.exit(1);
}

