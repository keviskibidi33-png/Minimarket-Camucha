# Correcciones Aplicadas para Resolver Error NG0203

## Problema Identificado
El error NG0203 ocurre cuando Angular intenta usar `inject()` fuera del contexto de inyección válido. Esto puede suceder cuando:
- Se usa `inject()` en interceptores funcionales que se ejecutan fuera del contexto
- Se usa `effect()` dentro de `setTimeout` o callbacks asíncronos
- Se cambia de pestaña o se navega entre componentes lazy-loaded

## Soluciones Implementadas

### 1. Interceptores Convertidos a Clases ✅
**Archivos modificados:**
- `src/app/core/interceptors/auth.interceptor.ts`
- `src/app/core/interceptors/error.interceptor.ts`
- `src/app/app.config.ts`

**Cambios:**
- Convertidos de interceptores funcionales (`HttpInterceptorFn`) a clases (`HttpInterceptor`)
- Usa inyección de dependencias por constructor (más estable)
- Configurados usando `HTTP_INTERCEPTORS` provider en lugar de `withInterceptors()`

**Beneficios:**
- Mayor estabilidad y mejor manejo del contexto de inyección
- Mejor para debugging y testing
- Sigue las mejores prácticas de Angular

### 2. PermissionsService Mejorado ✅
**Archivo modificado:**
- `src/app/core/services/permissions.service.ts`

**Cambios:**
- Eliminado `afterNextRender` y `setTimeout` innecesarios
- Agregado `DestroyRef` para limpieza adecuada del `effect()`
- Mejor manejo del ciclo de vida del effect

**Beneficios:**
- El effect se ejecuta solo en el constructor cuando el contexto es válido
- Limpieza automática cuando el servicio se destruye
- Menos código y más mantenible

### 3. PosComponent Corregido ✅
**Archivo modificado:**
- `src/app/features/pos/pos.component.ts`

**Cambios:**
- Eliminado `setTimeout` innecesario dentro de `afterNextRender`
- El `effect()` ahora se ejecuta directamente después de `afterNextRender`
- Agregado `DestroyRef` para limpieza del effect

**Beneficios:**
- El effect se ejecuta en el contexto correcto
- No hay ejecuciones fuera de la zona de Angular
- Limpieza adecuada cuando el componente se destruye

### 4. ShippingComponent Mejorado ✅
**Archivo modificado:**
- `src/app/features/store/checkout/shipping/shipping.component.ts`

**Cambios:**
- Reemplazado `requestAnimationFrame` con `NgZone.run()`
- Asegura que el código se ejecute dentro de la zona de Angular

**Beneficios:**
- Mejor detección de cambios
- Código más predecible y mantenible

### 5. Guards Mantenidos (Funcionales) ✅
**Archivos:**
- `src/app/core/guards/auth.guard.ts`
- `src/app/core/guards/role.guard.ts`

**Nota:** Los guards funcionales están bien porque Angular garantiza que se ejecuten en el contexto correcto cuando se activan las rutas.

## Mejores Prácticas Aplicadas

1. **Inyección de Dependencias:**
   - Preferir inyección por constructor en servicios e interceptores
   - Usar `inject()` solo en guards funcionales y componentes modernos cuando sea necesario

2. **Effects:**
   - Ejecutar `effect()` solo en el constructor o en métodos de inicialización
   - Nunca usar `effect()` dentro de `setTimeout` o callbacks asíncronos sin contexto
   - Siempre limpiar effects usando `DestroyRef.onDestroy()`

3. **Zona de Angular:**
   - Usar `NgZone.run()` para ejecutar código fuera de la zona dentro de la zona
   - Evitar `requestAnimationFrame` y `setTimeout` cuando sea posible

4. **Interceptores:**
   - Preferir interceptores basados en clases para mayor estabilidad
   - Usar `HTTP_INTERCEPTORS` provider en lugar de `withInterceptors()`

## Verificación

Para verificar que las correcciones funcionan:

1. **Compilar el proyecto:**
   ```bash
   npm start
   ```

2. **Probar navegación:**
   - Cambiar entre pestañas del navegador
   - Navegar entre rutas
   - Cargar componentes lazy-loaded

3. **Verificar consola:**
   - No debería aparecer el error NG0203
   - Las peticiones HTTP deberían funcionar correctamente
   - Los guards deberían funcionar sin problemas

## Notas Adicionales

- Los interceptores funcionales pueden funcionar en algunos casos, pero las clases son más estables
- El error NG0203 puede persistir si hay código legacy que no se ha actualizado
- Siempre limpiar subscriptions y effects para evitar memory leaks

## Correcciones Adicionales Aplicadas

### 6. StoreHeaderComponent Corregido ✅
**Archivo modificado:**
- `src/app/shared/components/store-header/store-header.component.ts`

**Cambios:**
- Eliminado `effect()` dentro de `setTimeout` dentro de `afterNextRender`
- Reemplazado por `computed()` que es más eficiente y no requiere contexto de inyección
- Eliminadas variables no utilizadas

**Beneficios:**
- No hay ejecución de código fuera del contexto de Angular
- Mejor rendimiento con `computed`
- Código más limpio y mantenible

### 7. SendReceiptDialogComponent Corregido ✅
**Archivo modificado:**
- `src/app/shared/components/send-receipt-dialog/send-receipt-dialog.component.ts`

**Cambios:**
- Eliminado `setTimeout` innecesario dentro de `afterNextRender`
- El `effect()` ahora se ejecuta directamente en el constructor
- Agregado `DestroyRef` para limpieza adecuada

**Beneficios:**
- El effect se ejecuta en el contexto correcto
- Limpieza adecuada cuando el componente se destruye

### 8. ToastComponent Corregido ✅ (Causa Raíz del Error)
**Archivo modificado:**
- `src/app/shared/components/toast/toast.component.ts`

**Problema identificado:**
- El `setTimeout` en el método `show()` se ejecutaba fuera de la zona de Angular
- Esto causaba el error NG0203 cuando las notificaciones del carrito se mostraban
- El timeout no se limpiaba correctamente al destruir el componente

**Cambios:**
- Agregado `NgZone.run()` para ejecutar `setTimeout` dentro de la zona de Angular
- Agregado `DestroyRef` para limpiar timeouts cuando el componente se destruye
- Mejorado el manejo de timeouts para evitar múltiples timeouts activos

**Beneficios:**
- Las notificaciones ahora se ejecutan dentro del contexto correcto de Angular
- No hay pérdida de contexto de inyección
- Limpieza adecuada de recursos

## Análisis del Error Persistente

El error NG0203 **era causado por las notificaciones del carrito** que usaban `setTimeout` fuera de la zona de Angular. Esto ha sido corregido.

Si el error persiste después de todas las correcciones, puede ser debido a:

1. **Caché del navegador**: El código compilado anterior todavía está en caché
2. **Componentes lazy-loaded**: Algunos componentes se cargan después de la inicialización
3. **Guards funcionales**: Aunque están correctos, pueden ejecutarse en momentos inesperados

## Solución Final Recomendada

Si el error persiste después de todas las correcciones:

1. **Limpiar caché del navegador completamente**:
   - Presionar `Ctrl + Shift + Delete` en Chrome/Edge
   - Seleccionar "Caché de imágenes y archivos"
   - Hacer clic en "Borrar datos"
   - Recargar la página con `Ctrl + F5`

2. **Limpiar caché de Angular**:
   ```bash
   cd minimarket-web
   rm -rf .angular
   npm start
   ```

3. **Verificar que no haya código legacy**:
   - Buscar todos los usos de `inject()` en el proyecto
   - Asegurarse de que todos estén en contexto válido
   - Verificar que no haya `effect()` dentro de callbacks asíncronos

## Próximos Pasos Recomendados

1. ✅ Revisar otros componentes que usen `setTimeout` o `setInterval` - COMPLETADO
2. ✅ Verificar que todos los effects tengan limpieza adecuada - COMPLETADO
3. ✅ Considerar usar `computed` en lugar de `effect` cuando sea posible - COMPLETADO
4. Agregar tests unitarios para interceptores y guards
5. Monitorear el error en producción para identificar patrones específicos

