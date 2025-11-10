# Credenciales de Usuario Administrador

## Credenciales de Acceso

### Usuario Administrador
- **Email**: `admin@minimarketcamucha.com`
- **Username**: `admin`
- **Contraseña**: `Admin123!`
- **Rol**: Administrador

### Otros Usuarios del Sistema

#### Cajero
- **Email**: `cajero@minimarketcamucha.com`
- **Username**: `cajero`
- **Contraseña**: `Cajero123!`
- **Rol**: Cajero

#### Almacenero
- **Email**: `almacenero@minimarketcamucha.com`
- **Username**: `almacenero`
- **Contraseña**: `Almacenero123!`
- **Rol**: Almacenero

## Notas Importantes

1. **Login**: El sistema permite iniciar sesión usando el **email** o el **username**.
2. **Perfil**: Los usuarios admin, cajero y almacenero fueron creados antes del sistema de perfiles, por lo que no tienen `UserProfile` asociado. Esto es normal y no afecta su funcionalidad.
3. **Creación Automática**: Estos usuarios se crean automáticamente al ejecutar el seeder de la base de datos.

## Cómo Acceder

1. Ve a `http://localhost:4200/auth/login`
2. Ingresa el **email** o **username** del administrador
3. Ingresa la contraseña
4. Serás redirigido al panel de administración

## Cambiar Contraseña

Si necesitas cambiar la contraseña del administrador, puedes hacerlo desde:
- El panel de administración (si está implementado)
- Directamente en la base de datos
- Usando el script de PowerShell para resetear contraseña

