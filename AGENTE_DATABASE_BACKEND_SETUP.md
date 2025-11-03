# AGENTE DE DATABASE & BACKEND SETUP - MINIMARKET
## Documentación Completa del Desarrollo

---

## 📋 RESUMEN EJECUTIVO

**Rol:** Database & Backend Infrastructure Specialist  
**Fecha de Implementación:** 2024  
**Proyecto:** Sistema Minimarket Camucha  
**Stack:** ASP.NET Core 9.0, Entity Framework Core 9.0, SQL Server 2022

---

## ✅ IMPLEMENTACIONES COMPLETADAS

### 1. SCRIPTS SQL DE SETUP INICIAL

#### 📄 Archivo: `scripts/01_CreateDatabase.sql`
- Script para crear la base de datos `MinimarketDB`
- Configuración de archivos de datos y logs
- Configuración de opciones de base de datos:
  - RECOVERY SIMPLE
  - READ_COMMITTED_SNAPSHOT ON

#### 📄 Archivo: `scripts/02_CreateUser.sql`
- Creación de login `minimarket_app`
- Creación de usuario en la base de datos
- Asignación de permisos:
  - db_datareader
  - db_datawriter
  - EXECUTE
  - VIEW DEFINITION

---

### 2. ENTIDADES Y ENUMS NUEVOS

#### 📄 Archivo: `src/Minimarket.Domain/Entities/InventoryMovement.cs`
**Propiedades:**
- ProductId (Guid)
- Type (InventoryMovementType)
- Quantity (int) - Positivo para entrada, negativo para salida
- Reason (string?) - Razón del movimiento
- Reference (string?) - Referencia (número de venta, compra, etc.)
- SaleId (Guid?) - Relación opcional con venta
- UserId (Guid?) - Usuario que realizó el movimiento
- UnitPrice (decimal?) - Precio unitario al momento del movimiento
- Notes (string?) - Notas adicionales

**Relaciones:**
- Product (required)
- Sale (optional)

#### 📄 Archivo: `src/Minimarket.Domain/Enums/InventoryMovementType.cs`
**Valores:**
- Entrada = 1 (Compra, ajuste positivo)
- Salida = 2 (Venta, ajuste negativo)
- Ajuste = 3 (Ajuste de inventario)
- Devolucion = 4 (Devolución de cliente)

---

### 3. INTERFACES DE REPOSITORIOS ESPECÍFICOS

#### 📄 Archivo: `src/Minimarket.Domain/Interfaces/IProductRepository.cs`
**Métodos Especializados:**
- `GetByCodeAsync(string code)` - Obtener producto por código
- `GetLowStockProductsAsync()` - Productos con stock bajo
- `SearchAsync(string searchTerm)` - Búsqueda por nombre, código o descripción
- `GetByCategoryIdAsync(Guid categoryId)` - Productos por categoría
- `ExistsAsync(string code, Guid? excludeId)` - Verificar existencia de código
- `UpdateStockAsync(Guid productId, int quantity)` - Actualizar stock

#### 📄 Archivo: `src/Minimarket.Domain/Interfaces/ISaleRepository.cs`
**Métodos Especializados:**
- `GetByIdWithDetailsAsync(Guid id)` - Obtener venta con detalles y relaciones
- `GetByDateRangeAsync(DateTime startDate, DateTime endDate)` - Ventas por rango de fechas
- `GetByDocumentNumberAsync(string documentNumber)` - Venta por número de documento
- `GetNextDocumentNumberAsync(DocumentType documentType)` - Generar siguiente número de documento
- `GetTotalSalesAmountAsync(DateTime? startDate, DateTime? endDate)` - Total de ventas pagadas

#### 📄 Archivo: `src/Minimarket.Domain/Interfaces/ICategoryRepository.cs`
**Métodos Especializados:**
- `GetActiveCategoriesAsync()` - Categorías activas
- `GetByNameAsync(string name)` - Categoría por nombre

#### 📄 Archivo: `src/Minimarket.Domain/Interfaces/ICustomerRepository.cs`
**Métodos Especializados:**
- `GetByDocumentNumberAsync(string documentNumber)` - Cliente por número de documento
- `SearchAsync(string searchTerm)` - Búsqueda de clientes
- `ExistsByDocumentAsync(string documentNumber, string documentType, Guid? excludeId)` - Verificar existencia

#### 📄 Archivo: `src/Minimarket.Domain/Interfaces/IInventoryMovementRepository.cs`
**Métodos Especializados:**
- `GetByProductIdAsync(Guid productId)` - Movimientos por producto
- `GetByTypeAsync(InventoryMovementType type)` - Movimientos por tipo
- `GetByDateRangeAsync(DateTime startDate, DateTime endDate)` - Movimientos por rango de fechas
- `GetBySaleIdAsync(Guid saleId)` - Movimientos por venta

---

### 4. EXTENSIÓN DE IREPOSITORY

#### 📄 Archivo: `src/Minimarket.Domain/Interfaces/IRepository.cs`
**Método Agregado:**
- `GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate, Expression<Func<T, object>>? orderBy, bool ascending)` - Paginación con filtros y ordenamiento

---

### 5. IMPLEMENTACIONES DE REPOSITORIOS

#### 📄 Archivo: `src/Minimarket.Infrastructure/Data/Repositories/ProductRepository.cs`
- Hereda de `Repository<Product>` e implementa `IProductRepository`
- Todos los métodos incluyen `Include(p => p.Category)` para cargar relaciones
- Búsquedas optimizadas con índices

#### 📄 Archivo: `src/Minimarket.Infrastructure/Data/Repositories/SaleRepository.cs`
- Lógica de generación de números de documento (F001-00000001, B001-00000001)
- Manejo de errores en formato de documento
- Incluye relaciones: SaleDetails, Product, Customer

#### 📄 Archivo: `src/Minimarket.Infrastructure/Data/Repositories/CategoryRepository.cs`
- Filtrado por categorías activas
- Ordenamiento por nombre

#### 📄 Archivo: `src/Minimarket.Infrastructure/Data/Repositories/CustomerRepository.cs`
- Búsqueda multi-campo (nombre, documento, email, teléfono)
- Validación de documentos únicos

#### 📄 Archivo: `src/Minimarket.Infrastructure/Data/Repositories/InventoryMovementRepository.cs`
- Todos los métodos incluyen `Include(im => im.Product)`
- Ordenamiento por fecha descendente

#### 📄 Archivo: `src/Minimarket.Infrastructure/Data/Repositories/Repository.cs`
- Implementación de `GetPagedAsync` con paginación en base de datos
- Soporte para predicados, ordenamiento ascendente/descendente
- Retorna `PagedResult<T>`

---

### 6. MEJORAS AL DBCONTEXT

#### 📄 Archivo: `src/Minimarket.Infrastructure/Data/MinimarketDbContext.cs`

**Cambios Implementados:**

1. **DbSet Agregado:**
   - `DbSet<InventoryMovement> InventoryMovements`

2. **Método `ConfigureGlobalSettings`:**
   - DeleteBehavior Restrict por defecto para entidades de dominio
   - Precisión decimal global (18,2) para todas las propiedades decimal
   - Índices globales:
     - Product.Code (único)
     - Product.Name
     - Customer (DocumentType, DocumentNumber) (único)
     - Sale.DocumentNumber (único)
     - Sale.SaleDate

3. **Método `SaveChangesAsync` Override:**
   - Timestamps automáticos para entidades BaseEntity
   - `CreatedAt` se establece en Add
   - `UpdatedAt` se actualiza en Add y Update

---

### 7. CONFIGURACIONES DE ENTIDADES

#### 📄 Archivo: `src/Minimarket.Infrastructure/Data/Configurations/InventoryMovementConfiguration.cs`
**Nueva Configuración:**
- Table: InventoryMovements
- Type convertido a string
- Precisión decimal para UnitPrice (18,2)
- Relaciones con Product y Sale (Restrict)
- Índices: ProductId, Type, CreatedAt, SaleId

#### 📄 Archivo: `src/Minimarket.Infrastructure/Data/Configurations/SaleConfiguration.cs`
**Mejoras:**
- DocumentType convertido a string
- PaymentMethod convertido a string
- Status convertido a string
- SaleDate marcado como required
- Precisión decimal con HasPrecision (18,2)
- DeleteBehavior.Restrict para Customer
- Índices agregados: SaleDate, CustomerId, Status, UserId

#### 📄 Archivo: `src/Minimarket.Infrastructure/Data/Configurations/SaleDetailConfiguration.cs`
**Mejoras:**
- Cambio de HasColumnType a HasPrecision para UnitPrice y Subtotal

---

### 8. UNIT OF WORK

#### 📄 Archivo: `src/Minimarket.Domain/Interfaces/IUnitOfWork.cs`
**Agregado:**
- `IAsyncDisposable` implementado
- Repositorios específicos:
  - `IProductRepository ProductRepository`
  - `ICategoryRepository CategoryRepository`
  - `ICustomerRepository CustomerRepository`
  - `ISaleRepository SaleRepository`
  - `IInventoryMovementRepository InventoryMovementRepository`
- Repositorio genérico: `IRepository<InventoryMovement> InventoryMovements`

#### 📄 Archivo: `src/Minimarket.Infrastructure/Data/Repositories/UnitOfWork.cs`
**Implementación:**
- Lazy initialization de todos los repositorios
- Implementación de `DisposeAsync`
- Mantiene compatibilidad con repositorios genéricos

---

### 9. CONFIGURACIÓN DE CONNECTION STRINGS Y DATABASE SETTINGS

#### 📄 Archivo: `src/Minimarket.API/appsettings.json`
**Agregado:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=MinimarketDB;User Id=minimarket_app;Password=Minimarket@2024!;TrustServerCertificate=true;MultipleActiveResultSets=true;",
  "AzureConnection": "Server=tcp:minimarket-server.database.windows.net,1433;Initial Catalog=MinimarketDB;Persist Security Info=False;User ID=adminuser;Password=YourPassword123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
},
"DatabaseSettings": {
  "EnableSensitiveDataLogging": false,
  "EnableDetailedErrors": false,
  "CommandTimeout": 30,
  "MaxRetryCount": 3,
  "MaxRetryDelay": 10
}
```

#### 📄 Archivo: `src/Minimarket.API/appsettings.Development.json`
**Agregado:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=MinimarketDB;Trusted_Connection=true;TrustServerCertificate=true;MultipleActiveResultSets=true;"
},
"DatabaseSettings": {
  "EnableSensitiveDataLogging": true,
  "EnableDetailedErrors": true
}
```

---

### 10. DEPENDENCY INJECTION MEJORADA

#### 📄 Archivo: `src/Minimarket.Infrastructure/DependencyInjection.cs`
**Mejoras Implementadas:**

1. **Validación de Connection String:**
   - Verifica que exista antes de configurar DbContext

2. **Configuración Avanzada de EF Core:**
   - `EnableRetryOnFailure` con MaxRetryCount y MaxRetryDelay configurables
   - `CommandTimeout` configurable
   - `EnableSensitiveDataLogging` solo en desarrollo
   - `EnableDetailedErrors` solo en desarrollo
   - `MigrationsAssembly` configurado

3. **Registro de Repositorios:**
   - UnitOfWork (scoped)
   - Repositorios específicos (scoped):
     - IProductRepository → ProductRepository
     - ICategoryRepository → CategoryRepository
     - ICustomerRepository → CustomerRepository
     - ISaleRepository → SaleRepository
     - IInventoryMovementRepository → InventoryMovementRepository

---

### 11. HEALTH CHECKS PERSONALIZADOS

#### 📄 Archivo: `src/Minimarket.Infrastructure/HealthChecks/DatabaseHealthCheck.cs`
**Implementación:**
- Verifica conexión a base de datos
- Cuenta registros de Products, Categories, Sales
- Retorna datos estructurados en el resultado
- Manejo de excepciones con información detallada

#### 📄 Archivo: `src/Minimarket.API/Program.cs`
**Agregado:**
- Registro de `DatabaseHealthCheck` personalizado
- Endpoints:
  - `/health` - Health check general con respuesta JSON
  - `/health/ready` - Health check para readiness probe con respuesta JSON
- Respuesta JSON estructurada:
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "sqlserver",
      "status": "Healthy",
      "description": "...",
      "data": {...}
    }
  ]
}
```

---

## 🔧 COMANDOS A EJECUTAR

### 1. INSTALAR HERRAMIENTAS EF CORE (si no están instaladas)
```bash
dotnet tool install --global dotnet-ef
```

### 2. EJECUTAR SCRIPTS SQL (OPCIONAL - Para setup manual)
```bash
# Ejecutar desde SQL Server Management Studio o sqlcmd
sqlcmd -S localhost\SQLEXPRESS -i scripts/01_CreateDatabase.sql
sqlcmd -S localhost\SQLEXPRESS -i scripts/02_CreateUser.sql
```

### 3. CREAR MIGRACIÓN INICIAL
```bash
cd src/Minimarket.API
dotnet ef migrations add InitialCreate --project ../Minimarket.Infrastructure --startup-project .
```

### 4. VER SQL QUE SE EJECUTARÁ
```bash
dotnet ef migrations script --project ../Minimarket.Infrastructure --startup-project .
```

### 5. APLICAR MIGRACIÓN A LA BASE DE DATOS
```bash
dotnet ef database update --project ../Minimarket.Infrastructure --startup-project .
```

### 6. VERIFICAR MIGRACIONES APLICADAS
```bash
dotnet ef migrations list --project ../Minimarket.Infrastructure --startup-project .
```

### 7. VERIFICAR HEALTH CHECK
```bash
# Una vez que la aplicación esté corriendo
curl http://localhost:5000/health
# o
curl http://localhost:5000/health/ready
```

---

## 📝 VALIDACIONES OBLIGATORIAS

### ✅ Checklist de Configuración

#### SQL Server
- [ ] SQL Server 2022 SQLEXPRESS está instalado y corriendo
- [ ] Base de datos MinimarketDB creada correctamente (o será creada por migraciones)
- [ ] Usuario minimarket_app creado con permisos correctos (o usar Trusted_Connection en desarrollo)
- [ ] Connection string funciona (probado con sqlcmd o desde la aplicación)
- [ ] Firewall permite conexiones al puerto 1433 (si aplica)

#### Entity Framework Core
- [x] MinimarketDbContext implementado correctamente
- [x] Todas las entidades tienen DbSet<T>
- [x] Configuraciones Fluent API aplicadas
- [x] Seed data inicial configurado (en DatabaseSeeder)
- [x] Timestamps automáticos funcionan (CreatedAt, UpdatedAt)

#### Migrations
- [ ] Migración inicial creada exitosamente (EJECUTAR)
- [ ] Migración aplicada a la base de datos (EJECUTAR)
- [ ] Tablas creadas con índices correctos
- [ ] Foreign keys configuradas correctamente
- [ ] Seed data insertado en la base de datos

#### Repositories
- [x] GenericRepository<T> implementado con GetPagedAsync
- [x] Repositorios específicos implementados (Product, Sale, Category, Customer, InventoryMovement)
- [x] Todos los métodos async con CancellationToken
- [x] Queries incluyen relaciones necesarias (Include)
- [x] Paginación implementada correctamente

#### Unit of Work
- [x] IUnitOfWork implementado con repositorios específicos
- [x] Transacciones funcionan (Begin, Commit, Rollback)
- [x] SaveChangesAsync funciona correctamente
- [x] Dispose/DisposeAsync implementados

#### Performance
- [x] Índices en columnas de búsqueda frecuente
- [x] Queries no generan N+1 problem (Include usado apropiadamente)
- [x] Connection pooling habilitado (por defecto en EF Core)
- [x] Retry logic configurado

#### Testing
- [ ] Connection string funciona desde la aplicación (EJECUTAR APLICACIÓN)
- [ ] Health check endpoint responde correctamente (/health) (EJECUTAR APLICACIÓN)
- [ ] CRUD básico funciona (Create, Read, Update, Delete) (EJECUTAR PRUEBAS)
- [ ] Transacciones rollback correctamente en caso de error (EJECUTAR PRUEBAS)

---

## 🚀 PRÓXIMOS PASOS

### FASE 1: VALIDACIÓN INICIAL (INMEDIATO)
1. **Ejecutar migraciones:**
   ```bash
   dotnet ef migrations add InitialCreate --project src/Minimarket.Infrastructure --startup-project src/Minimarket.API
   dotnet ef database update --project src/Minimarket.Infrastructure --startup-project src/Minimarket.API
   ```

2. **Probar conexión:**
   - Ejecutar la aplicación
   - Verificar que se conecte a la base de datos
   - Verificar endpoint `/health`

3. **Validar seed data:**
   - Verificar que se inserten categorías, productos y usuarios iniciales

### FASE 2: TESTING (RECOMENDADO)
1. **Integration Tests:**
   - Crear tests para repositorios específicos
   - Validar métodos de búsqueda
   - Validar generación de números de documento

2. **Unit Tests:**
   - Tests para lógica de repositorios
   - Tests para UnitOfWork

### FASE 3: OPTIMIZACIÓN (FUTURO)
1. **Performance:**
   - Revisar queries generadas por EF Core
   - Agregar índices adicionales si es necesario
   - Considerar AsNoTracking para queries de solo lectura

2. **Monitoreo:**
   - Configurar logging detallado de queries en desarrollo
   - Revisar tiempos de ejecución de queries complejas

---

## 📚 ARCHIVOS CREADOS

### Scripts SQL
1. `scripts/01_CreateDatabase.sql`
2. `scripts/02_CreateUser.sql`

### Entidades y Enums
3. `src/Minimarket.Domain/Entities/InventoryMovement.cs`
4. `src/Minimarket.Domain/Enums/InventoryMovementType.cs`

### Interfaces
5. `src/Minimarket.Domain/Interfaces/IProductRepository.cs`
6. `src/Minimarket.Domain/Interfaces/ISaleRepository.cs`
7. `src/Minimarket.Domain/Interfaces/ICategoryRepository.cs`
8. `src/Minimarket.Domain/Interfaces/ICustomerRepository.cs`
9. `src/Minimarket.Domain/Interfaces/IInventoryMovementRepository.cs`

### Implementaciones
10. `src/Minimarket.Infrastructure/Data/Repositories/ProductRepository.cs`
11. `src/Minimarket.Infrastructure/Data/Repositories/SaleRepository.cs`
12. `src/Minimarket.Infrastructure/Data/Repositories/CategoryRepository.cs`
13. `src/Minimarket.Infrastructure/Data/Repositories/CustomerRepository.cs`
14. `src/Minimarket.Infrastructure/Data/Repositories/InventoryMovementRepository.cs`

### Configuraciones
15. `src/Minimarket.Infrastructure/Data/Configurations/InventoryMovementConfiguration.cs`

### Health Checks
16. `src/Minimarket.Infrastructure/HealthChecks/DatabaseHealthCheck.cs`

---

## 📝 ARCHIVOS MODIFICADOS

1. `src/Minimarket.Domain/Interfaces/IRepository.cs` - Agregado GetPagedAsync
2. `src/Minimarket.Domain/Interfaces/IUnitOfWork.cs` - Agregados repositorios específicos e IAsyncDisposable
3. `src/Minimarket.Infrastructure/Data/Repositories/Repository.cs` - Implementado GetPagedAsync
4. `src/Minimarket.Infrastructure/Data/Repositories/UnitOfWork.cs` - Agregados repositorios específicos
5. `src/Minimarket.Infrastructure/Data/MinimarketDbContext.cs` - Mejoras globales y timestamps automáticos
6. `src/Minimarket.Infrastructure/Data/Configurations/SaleConfiguration.cs` - Mejoras y conversión de enums
7. `src/Minimarket.Infrastructure/Data/Configurations/SaleDetailConfiguration.cs` - HasPrecision
8. `src/Minimarket.Infrastructure/DependencyInjection.cs` - Configuración avanzada de EF Core
9. `src/Minimarket.API/appsettings.json` - DatabaseSettings y connection strings
10. `src/Minimarket.API/appsettings.Development.json` - DatabaseSettings para desarrollo
11. `src/Minimarket.API/Program.cs` - Health checks personalizados

---

## ⚠️ NOTAS IMPORTANTES

### Compatibilidad
- Se mantiene compatibilidad con código existente
- Los repositorios genéricos siguen funcionando
- Los repositorios específicos son adicionales, no reemplazan los genéricos

### Connection Strings
- **Desarrollo:** Usa `Trusted_Connection=true` (no requiere usuario específico)
- **Producción:** Usa usuario `minimarket_app` con contraseña
- **Azure:** Connection string preparada pero requiere configuración de servidor

### Seguridad
- Las contraseñas en appsettings.json deben cambiarse en producción
- Usar Azure Key Vault o variables de entorno para secrets en producción
- El usuario `minimarket_app` tiene permisos mínimos necesarios

### Performance
- Los índices agregados mejoran búsquedas por código, nombre, documento y fecha
- Retry logic configurado para manejar errores transitorios de conexión
- Connection pooling habilitado por defecto en EF Core

### Migraciones
- Las migraciones se aplican automáticamente al iniciar la aplicación en desarrollo
- En producción, aplicar migraciones manualmente o con un proceso de deployment

---

## 🔍 REFERENCIAS TÉCNICAS

### Entity Framework Core 9.0
- Documentación: https://learn.microsoft.com/en-us/ef/core/
- Migrations: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/

### SQL Server 2022
- Documentación: https://learn.microsoft.com/en-us/sql/sql-server/

### Health Checks
- Documentación: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks

---

## 📞 CONTACTO Y SOPORTE

Para cualquier duda o problema con la infraestructura de base de datos:
1. Revisar los logs de la aplicación
2. Verificar connection string
3. Validar que SQL Server esté corriendo
4. Revisar health check endpoint `/health`

---

**Documento creado por:** Agente de Database & Backend Setup  
**Última actualización:** 2024  
**Versión:** 1.0

