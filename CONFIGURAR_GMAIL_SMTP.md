# Guía Rápida: Configurar Gmail SMTP

## Pasos para Configurar Gmail

### 1. Habilitar Verificación en 2 Pasos

1. Ve a: https://myaccount.google.com/security
2. Busca "Verificación en 2 pasos"
3. Si no está activada, actívala (es obligatorio)

### 2. Crear Contraseña de Aplicación

1. Ve directamente a: https://myaccount.google.com/apppasswords
   - O desde: Google Account → Seguridad → Contraseñas de aplicaciones

2. Si no ves la opción:
   - Asegúrate de tener la verificación en 2 pasos activada
   - Puede que necesites verificar tu identidad

3. Crear la contraseña:
   - Selecciona "Correo" como aplicación
   - Selecciona "Otro (nombre personalizado)"
   - Escribe: **"Minimarket Camucha API"**
   - Haz clic en "Generar"

4. **Copia la contraseña generada** (16 caracteres, sin espacios)
   - Ejemplo: `abcd efgh ijkl mnop` → Usa: `abcdefghijklmnop`
   - ⚠️ Solo se muestra UNA VEZ, guárdala bien

### 3. Configurar en appsettings.json

Abre `src/Minimarket.API/appsettings.json` y configura:

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUser": "minimarket.camucha@gmail.com",
  "SmtpPassword": "abcdefghijklmnop",  // ← Pega aquí la contraseña de aplicación
  "FromEmail": "minimarket.camucha@gmail.com",
  "FromName": "Minimarket Camucha",
  "ApiKey": "",
  "ApiUrl": ""
}
```

### 4. Probar

1. Reinicia el backend
2. Completa un pedido desde el frontend
3. Revisa los logs del backend
4. Verifica el correo del cliente (y spam)

---

## Solución de Problemas

### Error: "Username and Password not accepted"
- ✅ Verifica que la verificación en 2 pasos esté activada
- ✅ Usa la contraseña de aplicación, NO tu contraseña normal
- ✅ Asegúrate de copiar la contraseña sin espacios

### Error: "Less secure app access"
- ✅ Gmail ya no permite "aplicaciones menos seguras"
- ✅ Debes usar "Contraseña de aplicación" (paso 2)

### No puedo acceder a apppasswords
- ✅ Verifica que la verificación en 2 pasos esté activada
- ✅ Puede requerir verificación adicional de Google
- ✅ Intenta desde otro navegador o modo incógnito

---

## Límites de Gmail

- **Gratis**: 500 emails/día
- **Google Workspace**: 2,000 emails/día

Para más volumen, considera:
- Resend (con dominio verificado)
- SendGrid
- Mailgun
- Amazon SES

