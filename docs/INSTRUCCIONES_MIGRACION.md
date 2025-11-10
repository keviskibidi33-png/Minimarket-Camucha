# Instrucciones para Crear la Migración

## ⚠️ IMPORTANTE

El servidor backend está corriendo y bloqueando los archivos DLL. Para crear la migración, necesitas:

1. **Detener el servidor backend** (Ctrl+C en la terminal donde está corriendo)
2. **Ejecutar el comando de migración**:
   ```powershell
   dotnet ef migrations add AddPaymentAndDeliverySettingsToBrandSettings --project src/Minimarket.Infrastructure --startup-project src/Minimarket.API
   ```
3. **Aplicar la migración**:
   ```powershell
   dotnet ef database update --project src/Minimarket.Infrastructure --startup-project src/Minimarket.API
   ```

## Cambios Realizados

### Backend
- ✅ Agregados campos a `BrandSettings`:
  - `YapeEnabled`, `PlinEnabled` (visibilidad)
  - `BankName`, `BankAccountType`, `BankAccountNumber`, `BankCCI`, `BankAccountVisible`
  - `DeliveryType`, `DeliveryCost`, `DeliveryZones`
- ✅ Actualizado `BrandSettingsConfiguration` con los nuevos campos
- ✅ Actualizado `AdminSetupCommandHandler` para guardar los nuevos campos
- ✅ Actualizado `UpdateBrandSettingsCommandHandler` y `GetBrandSettingsQueryHandler`
- ✅ Actualizado DTOs (`BrandSettingsDto`, `UpdateBrandSettingsDto`)
- ✅ Corregido `AuthController` para usar `GetFile()` en lugar de `ContainsKey()`

### Frontend
- ✅ Creado paso 4: "Información de Pago y Envío" en el wizard
- ✅ Agregados campos para Yape/Plin (teléfono y QR)
- ✅ Agregados campos para cuenta bancaria
- ✅ Agregadas opciones de envío (tipo, costo, zonas)
- ✅ Mejorados mensajes de validación del paso 1

## Próximos Pasos

1. **Crear la migración** (cuando el servidor esté detenido)
2. **Aplicar la migración** a la base de datos
3. **Crear sección de admin** para gestionar visibilidad de métodos de pago
4. **Integrar opciones de envío** con el carrito de compras
5. **Crear sección de configuración de sistema** para personalizar página

