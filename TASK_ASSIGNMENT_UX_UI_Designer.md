# TASK ASSIGNMENT - UX/UI Designer - Categories Form and UI Improvements

**Fecha**: [Fecha Actual]  
**Agente**: @UX-UI-Designer  
**Prioridad**: 🟡 MEDIA  
**Deadline**: Esta semana (3 días hábiles después de que backend esté listo)

---

## CONTEXTO Y OBJETIVO

Como UX/UI Designer, eres responsable del diseño visual y experiencia de usuario del frontend. Actualmente el CRUD de Categorías está incompleto en frontend y hay mejoras pendientes en paginación y loading states.

**Objetivo**: Implementar formulario de categorías con diseño consistente y mejorar experiencia de usuario en componentes existentes.

---

## RESPONSABILIDADES DE UX/UI DESIGNER

### 1. Diseño Visual
- Mantener consistencia con design system existente
- Seguir patrones de diseño establecidos
- Asegurar responsive design

### 2. Experiencia de Usuario
- Mejorar feedback visual (loading, errors, success)
- Optimizar flujos de usuario
- Mejorar accesibilidad

### 3. Implementación
- Crear componentes reutilizables
- Mantener código limpio y mantenible
- Seguir estándares de Angular

---

## TAREAS ASIGNADAS

### TAREA 1: Formulario de Categorías (Día 1-2 - 5 horas)

**PRIORITY**: 🟡 MEDIA  
**DELIVERABLE**: Formulario completo de crear/editar categorías

#### Acceptance Criteria:
- [ ] Crear componente `category-form.component.ts`
- [ ] Crear template HTML con diseño consistente con otros formularios
- [ ] Implementar Reactive Forms
- [ ] Validaciones en tiempo real
- [ ] Mensajes de error claros y visibles
- [ ] Loading state durante guardado
- [ ] Toast de éxito/error al guardar
- [ ] Botón cancelar que regresa al listado
- [ ] Diseño responsive (funciona en tablet)
- [ ] Sigue design system (colores, tipografía, espaciado)
- [ ] Accesibilidad: labels, ARIA attributes

#### Reference Files:
- `minimarket-web/src/app/features/products/product-form/` (para referencia de diseño)
- `minimarket-web/src/app/features/customers/customer-form/` (para referencia de diseño)
- `minimarket-web/src/app/features/categories/categories.component.ts` (listado existente)

#### Campos del Formulario:
- **Nombre**: Text input, requerido, máximo 100 caracteres
- **Descripción**: Textarea, opcional, máximo 500 caracteres
- **Activo**: Checkbox (por defecto activo)

#### Design Requirements:
- Usar Angular Material components
- Colores: Azul (#0d7ff2) para admin
- Espaciado consistente con otros formularios
- Botones con iconos (Material Symbols)
- Validación visual en campos (rojo cuando hay error)

---

### TAREA 2: Mejoras en Paginación (Día 2 - 3 horas)

**PRIORITY**: 🟡 MEDIA  
**DELIVERABLE**: Paginación mejorada mostrando total real

#### Acceptance Criteria:
- [ ] Modificar servicios para retornar total count
- [ ] Actualizar componentes de listado (Products, Customers, Sales)
- [ ] Mostrar "Mostrando X de Y" en lugar de solo página actual
- [ ] Botones de paginación funcionan correctamente
- [ ] Diseño consistente en todos los listados
- [ ] Responsive (funciona en mobile)

#### Components a Actualizar:
- `products.component.ts` y `products.component.html`
- `customers.component.ts` y `customers.component.html`
- `sales.component.ts` y `sales.component.html`

#### Reference Files:
- `src/Minimarket.Application/Common/Models/PagedResult.cs` (verificar si tiene TotalCount)

#### Implementation:
```typescript
// En componente de listado
totalCount = signal<number>(0);
currentPage = signal<number>(1);
pageSize = signal<number>(10);

// En template
<div class="pagination-info">
  Mostrando {{ (currentPage() - 1) * pageSize() + 1 }} - 
  {{ Math.min(currentPage() * pageSize(), totalCount()) }} 
  de {{ totalCount() }} resultados
</div>
```

---

### TAREA 3: Mejoras en Loading States (Día 3 - 2 horas)

**PRIORITY**: 🟡 MEDIA  
**DELIVERABLE**: Loading states mejorados y consistentes

#### Acceptance Criteria:
- [ ] Loading spinner durante carga de datos
- [ ] Skeleton loaders en tablas (opcional pero mejor UX)
- [ ] Loading state en botones durante acciones (guardar, eliminar)
- [ ] Deshabilitar formularios durante guardado
- [ ] Loading state consistente en toda la aplicación
- [ ] No hay "flash" de contenido vacío

#### Components a Mejorar:
- Todos los componentes de listado
- Todos los formularios
- Componente POS

#### Implementation:
```typescript
// Ejemplo de loading state en botón
<button 
  [disabled]="isLoading()" 
  [class.opacity-50]="isLoading()">
  @if (isLoading()) {
    <span class="spinner"></span>
  } @else {
    <span>Guardar</span>
  }
</button>
```

---

### TAREA 4: Mejoras en Mensajes de Error (Día 3 - 2 horas)

**PRIORITY**: 🟡 MEDIA  
**DELIVERABLE**: Mensajes de error más user-friendly

#### Acceptance Criteria:
- [ ] Mensajes de error claros y específicos
- [ ] Mensajes de error visibles (no se ocultan rápidamente)
- [ ] Mensajes de error en español (consistente)
- [ ] Validaciones de formulario muestran errores inline
- [ ] Errores de API se muestran en toast
- [ ] Mensajes de error accesibles (ARIA)

#### Implementation:
```typescript
// Ejemplo de mensaje de error inline
<div class="error-message" *ngIf="form.get('name')?.hasError('required') && form.get('name')?.touched">
  El nombre es requerido
</div>
```

---

## ESTRUCTURA DE ARCHIVOS

```
minimarket-web/src/app/features/categories/
├── category-form/
│   ├── category-form.component.ts (CREAR)
│   ├── category-form.component.html (CREAR)
│   └── category-form.component.css (CREAR)
├── categories.component.ts (MEJORAR - agregar navegación a form)
└── categories.component.html (MEJORAR - agregar botón crear)
```

---

## ESTÁNDARES DE DISEÑO

### Design System
- **Colores Admin**: Azul (#0d7ff2)
- **Colores Tienda**: Verde (#4CAF50)
- **Tipografía**: Seguir sistema existente
- **Espaciado**: Consistente (usar Tailwind spacing)
- **Iconos**: Material Symbols

### Componentes Angular Material
- `mat-form-field` para inputs
- `mat-button` para botones
- `mat-card` para contenedores
- `mat-dialog` para confirmaciones (ya existe)

### Responsive Design
- **Mobile**: Stack vertical, botones full-width
- **Tablet**: Layout adaptativo
- **Desktop**: Layout completo con sidebar

### Accesibilidad
- Labels para todos los inputs
- ARIA attributes donde sea necesario
- Navegación por teclado funcional
- Contraste adecuado (WCAG AA)

---

## MÉTRICAS Y OBJETIVOS

### Quality Metrics
- **Consistencia**: 100% con otros formularios
- **Responsive**: Funciona en mobile/tablet/desktop
- **Accesibilidad**: WCAG AA mínimo
- **Performance**: Sin lag en interacciones

---

## DEPENDENCIAS Y BLOQUEOS

### Dependencias
- ⚠️ **BLOQUEADO HASTA**: Backend complete CRUD Categorías
- ✅ Diseños de referencia disponibles
- ✅ Componentes reutilizables disponibles

### Bloqueos Potenciales
- Si backend no está listo, puede preparar estructura y diseño
- Si falta información de API, consultar con Tech Lead

### Acción si Bloqueado
- Preparar estructura y diseño mientras espera backend
- Crear mock data para desarrollo frontend
- Reportar a Tech Lead si bloqueo persiste >1 día

---

## REPORTE DIARIO REQUERIDO

Al final de cada día, reportar:

```
## DAILY PROGRESS - UX/UI Designer - [Fecha]

### Componentes Completados Hoy:
- [Lista de componentes]

### Mejoras Implementadas:
- [Lista de mejoras]

### Blockers:
- [Lista de blockers si los hay]

### Plan Mañana:
- [Tareas específicas para mañana]
```

---

## ACCEPTANCE CRITERIA FINAL

El trabajo está **COMPLETO** cuando:

- [ ] ✅ Formulario de categorías implementado y funcionando
- [ ] ✅ Diseño consistente con otros formularios
- [ ] ✅ Validaciones en tiempo real funcionan
- [ ] ✅ Paginación mejorada en todos los listados
- [ ] ✅ Loading states mejorados y consistentes
- [ ] ✅ Mensajes de error user-friendly
- [ ] ✅ Responsive design verificado
- [ ] ✅ Accesibilidad básica verificada
- [ ] ✅ Sin regresiones en componentes existentes
- [ ] ✅ PR creado con todos los cambios
- [ ] ✅ Code review aprobado por Tech Lead

---

## RECURSOS Y REFERENCIAS

### Archivos de Referencia
- `minimarket-web/src/app/features/products/product-form/` - Diseño de referencia
- `minimarket-web/src/app/features/customers/customer-form/` - Diseño de referencia
- `minimarket-web/src/app/shared/components/` - Componentes reutilizables

### Documentación
- [Angular Material](https://material.angular.io/)
- [Tailwind CSS](https://tailwindcss.com/)
- [WCAG Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)

---

## PRIORIZACIÓN DE TAREAS

**Orden de Ejecución Recomendado**:
1. **Día 1**: Preparar estructura mientras espera backend → Comenzar Tarea 1 (Category Form)
2. **Día 2**: Completar Tarea 1 → Tarea 2 (Paginación)
3. **Día 3**: Tarea 3 (Loading States) → Tarea 4 (Mensajes de Error)

---

## NOTAS FINALES

**@UX-UI-Designer**: 

Esta tarea tiene **prioridad media** porque está bloqueada por el backend. Sin embargo, puedes **preparar la estructura y el diseño** mientras esperas.

**ENFÓCATE EN**:
- ✅ Consistencia con diseño existente
- ✅ Experiencia de usuario fluida
- ✅ Responsive design
- ✅ Accesibilidad básica

**ESTA TAREA ESTÁ BLOQUEADA POR BACKEND PERO PUEDES AVANZAR EN DISEÑO Y ESTRUCTURA.**

---

**ASIGNADO POR**: Tech Lead  
**FECHA**: [Fecha Actual]  
**DEADLINE**: [Fecha + 3 días hábiles después de backend]  
**STATUS**: 🟡 BLOQUEADO - Esperando backend CRUD Categorías

