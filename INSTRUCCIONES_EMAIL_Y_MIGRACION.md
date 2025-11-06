# Instrucciones para Migración de Base de Datos y Configuración de Email

## 1. Migración de Base de Datos

### Crear la migración:
```bash
cd src/Minimarket.API
dotnet ef migrations add AddWebOrders --project ../Minimarket.Infrastructure --startup-project .
```

### Aplicar la migración:
```bash
dotnet ef database update --project ../Minimarket.Infrastructure --startup-project .
```

**Nota:** Si el backend está ejecutándose, deténlo primero antes de crear/aplicar la migración.

---

## 2. Configuración de Email

### Opción 1: Gmail SMTP (Recomendada para empezar) ⭐

**Ventajas:**
- ✅ No requiere dominio propio
- ✅ Gratis
- ✅ Fácil de configurar
- ✅ Funciona inmediatamente

**Pasos:**

1. **Habilitar verificación en 2 pasos en Gmail:**
   - Ve a: https://myaccount.google.com/security
   - Activa "Verificación en 2 pasos" si no la tienes

2. **Crear Contraseña de aplicación:**
   - Ve a: https://myaccount.google.com/apppasswords
   - O directamente: https://myaccount.google.com/apppasswords
   - Selecciona "Correo" y "Otro (nombre personalizado)"
   - Escribe: "Minimarket Camucha API"
   - Copia la contraseña generada (16 caracteres sin espacios)

3. **Configurar en `appsettings.json`:**
   ```json
   "EmailSettings": {
     "SmtpServer": "smtp.gmail.com",
     "SmtpPort": "587",
     "SmtpUser": "minimarket.camucha@gmail.com",
     "SmtpPassword": "tu-contraseña-de-aplicacion-aqui",  // ← Pega la contraseña de aplicación
     "FromEmail": "minimarket.camucha@gmail.com",
     "FromName": "Minimarket Camucha",
     "ApiKey": "",
     "ApiUrl": ""
   }
   ```

**⚠️ IMPORTANTE:**
- NO uses tu contraseña normal de Gmail
- Usa SOLO la "Contraseña de aplicación" generada
- Si no funciona, verifica que la verificación en 2 pasos esté activada

---

### Opción 2: Resend (Requiere dominio verificado)

**Ventajas:**
- ✅ Plan gratuito: 3,000 emails/mes
- ✅ API moderna
- ✅ Excelente deliverability

**Desventajas:**
- ❌ Requiere verificar dominio propio
- ❌ No funciona con Gmail sin dominio

**Pasos:**

1. **Crear cuenta en Resend:**
   - Ve a: https://resend.com
   - Crea una cuenta gratuita

2. **Verificar dominio:**
   - En "Domains", agrega tu dominio
   - Configura los registros DNS según las instrucciones
   - Esto es obligatorio para usar Resend

3. **Obtener API Key:**
   - En el dashboard, ve a "API Keys"
   - Crea una nueva API key
   - Copia la clave (empieza con `re_...`)

4. **Configurar en `appsettings.json`:**
   ```json
   "EmailSettings": {
     "SmtpServer": "",
     "SmtpPort": "587",
     "SmtpUser": "",
     "SmtpPassword": "",
     "FromEmail": "noreply@tudominio.com",
     "FromName": "Minimarket Camucha",
     "ApiKey": "re_tu_api_key_aqui",
     "ApiUrl": "https://api.resend.com/emails"
   }
   ```

---

### Opción 3: Otros servicios SMTP

Si prefieres usar otro proveedor SMTP:

**Outlook/Hotmail:**
```json
"SmtpServer": "smtp-mail.outlook.com",
"SmtpPort": "587",
"SmtpUser": "tu-email@outlook.com",
"SmtpPassword": "tu-contraseña"
```

**Yahoo:**
```json
"SmtpServer": "smtp.mail.yahoo.com",
"SmtpPort": "587",
"SmtpUser": "tu-email@yahoo.com",
"SmtpPassword": "tu-contraseña-de-aplicacion"
```

**Nota importante:**
- Gmail y Yahoo requieren "Contraseña de aplicación" (no la contraseña normal)
- Outlook puede requerir autenticación de aplicación también

---

## 3. Verificación

Una vez configurado, cuando un cliente confirme un pedido:
1. El pedido se guardará en la base de datos
2. Se enviará automáticamente un correo de confirmación al cliente
3. El correo incluirá:
   - Número de pedido
   - Detalles del pedido
   - Método de envío
   - Fecha estimada de entrega
   - Total del pedido

---

## 4. Pruebas

Para probar el envío de correos:
1. Completa un pedido desde el frontend
2. Revisa los logs del backend para ver si el correo se envió
3. Verifica la bandeja de entrada (y spam) del email del cliente

---

## 5. Monitoreo

- **Resend Dashboard**: Puedes ver estadísticas de envío en https://resend.com/emails
- **Logs del Backend**: Revisa los logs para ver confirmaciones de envío

