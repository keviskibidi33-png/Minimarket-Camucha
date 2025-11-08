# Problema de Parsing en Paginación con @for

## Descripción del Problema

Angular 18 tiene problemas para parsear elementos `<button>` dentro de bloques `@for` cuando están en ciertas estructuras. El error reportado es:

```
NG5002: Opening tag "button" not terminated.
NG5002: Unexpected closing tag "button".
```

## Causa Raíz

El compilador de Angular 18 puede tener problemas para parsear correctamente elementos HTML complejos dentro de bloques `@for`, especialmente cuando:
- El botón tiene múltiples atributos en múltiples líneas
- El botón está directamente dentro del `@for` sin un wrapper adicional
- Hay muchos atributos de clase condicionales

## Solución Implementada

Se creó un **componente de paginación reutilizable** (`PaginationComponent`) que:
1. ✅ Aísla la lógica de paginación
2. ✅ Evita problemas de parsing al estar en un componente separado
3. ✅ Es reutilizable en toda la aplicación
4. ✅ Sigue buenas prácticas de Angular

## Uso del Componente

```html
<app-pagination
  [currentPage]="currentPage()"
  [totalItems]="filteredOrders().length"
  [itemsPerPage]="itemsPerPage"
  (pageChange)="currentPage.set($event)">
</app-pagination>
```

## Scripts de Validación y Corrección

### Validar Templates
```bash
npm run validate:templates
```

Este script verifica:
- Tags HTML no cerrados
- Problemas de estructura en `@for`
- Botones problemáticos dentro de loops

### Corregir Paginación
```bash
npm run fix:pagination
```

Este script reemplaza automáticamente código problemático de paginación con el componente `<app-pagination>`.

## Buenas Prácticas Aplicadas

1. ✅ **Componentes Reutilizables**: La paginación está en un componente separado
2. ✅ **Utilidades Compartidas**: Funciones comunes en `shared/utils/order.utils.ts`
3. ✅ **Eliminación de Código Duplicado**: Métodos de formateo centralizados
4. ✅ **Estructura Limpia**: Separación de responsabilidades

## Prevención Futura

Para evitar este problema en el futuro:
- Usa componentes separados para lógica compleja dentro de `@for`
- Evita botones con muchos atributos directamente en `@for`
- Usa `track $index` en lugar de `track item` cuando sea posible
- Considera usar `@if` dentro de `@for` para estructuras más complejas

