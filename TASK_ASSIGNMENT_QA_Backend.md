# TASK ASSIGNMENT - QA Backend - Testing Implementation

**Fecha**: [Fecha Actual]  
**Agente**: @QA-Backend  
**Prioridad**: 🔴 CRÍTICA  
**Deadline**: Esta semana (5 días hábiles)

---

## CONTEXTO Y OBJETIVO

Como QA Backend, eres responsable de garantizar la calidad del código backend mediante la implementación de tests automatizados. El proyecto actualmente tiene **0% code coverage**, lo cual es **CRÍTICO** y debe resolverse urgentemente.

**Objetivo**: Implementar suite completa de tests que alcance **>80% coverage** en la capa Application, especialmente en lógica de negocio crítica.

---

## RESPONSABILIDADES DE QA BACKEND

### 1. Testing Structure
- Organizar tests en proyectos apropiados (Unit, Integration, Functional)
- Seguir convenciones de naming consistentes
- Mantener estructura de carpetas clara

### 2. Test Implementation
- Escribir tests unitarios para handlers y validadores
- Escribir integration tests para endpoints API
- Escribir tests funcionales para flujos completos

### 3. Code Coverage
- Alcanzar >80% coverage en Application layer
- Identificar y cubrir casos edge
- Documentar casos de prueba

### 4. Quality Assurance
- Verificar que todos los tests pasen
- Mantener tests actualizados con cambios de código
- Reportar bugs encontrados durante testing

---

## TAREAS ASIGNADAS

### TAREA 1: Setup Testing Infrastructure (Día 1 - 2 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Proyectos de tests configurados y funcionando

#### Acceptance Criteria:
- [ ] Verificar que `Minimarket.UnitTests` tiene frameworks instalados (xUnit, Moq, FluentAssertions)
- [ ] Verificar que `Minimarket.IntegrationTests` tiene WebApplicationFactory configurado
- [ ] Crear clase base `BaseIntegrationTest` con setup de DbContext en memoria o test database
- [ ] Configurar `TestFixture` para tests integration
- [ ] Crear helpers para seed de datos de prueba
- [ ] Verificar que todos los proyectos compilan correctamente

#### Reference Files:
- `tests/Minimarket.UnitTests/Minimarket.UnitTests.csproj`
- `tests/Minimarket.IntegrationTests/Minimarket.IntegrationTests.csproj`
- `src/Minimarket.API/Program.cs` (para WebApplicationFactory)

#### Implementation Details:
```csharp
// tests/Minimarket.IntegrationTests/BaseIntegrationTest.cs
public class BaseIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    
    public BaseIntegrationTest(WebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }
}
```

---

### TAREA 2: Unit Tests - Specifications (Día 1 - 3 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Tests unitarios completos para todas las especificaciones

#### Acceptance Criteria:
- [ ] Tests para `ProductHasSufficientStockSpecification`:
  - [ ] Test: Stock suficiente retorna true
  - [ ] Test: Stock insuficiente retorna false
  - [ ] Test: Producto inactivo retorna false
  - [ ] Test: Producto null retorna false
  - [ ] Test: Cantidad requerida <= 0 lanza ArgumentException
  - [ ] Test: ToExpression() retorna expresión correcta
  
- [ ] Tests para `ProductIsActiveSpecification`:
  - [ ] Test: Producto activo retorna true
  - [ ] Test: Producto inactivo retorna false
  - [ ] Test: Producto null retorna false
  - [ ] Test: ToExpression() retorna expresión correcta

- [ ] Tests para `SaleCanBeCancelledSpecification`:
  - [ ] Test: Venta Pendiente puede anularse (true)
  - [ ] Test: Venta Pagado puede anularse (true)
  - [ ] Test: Venta Anulado NO puede anularse (false)
  - [ ] Test: Venta null retorna false
  - [ ] Test: ToExpression() retorna expresión correcta

#### Coverage Target: 100% de especificaciones

#### Reference Files:
- `src/Minimarket.Domain/Specifications/ISpecification.cs`
- `src/Minimarket.Domain/Specifications/ProductHasSufficientStockSpecification.cs`
- `src/Minimarket.Domain/Specifications/ProductIsActiveSpecification.cs`
- `src/Minimarket.Domain/Specifications/SaleCanBeCancelledSpecification.cs`

#### Example Test Structure:
```csharp
// tests/Minimarket.UnitTests/Domain/Specifications/ProductHasSufficientStockSpecificationTests.cs
public class ProductHasSufficientStockSpecificationTests
{
    [Fact]
    public void IsSatisfiedBy_WhenStockIsSufficient_ReturnsTrue()
    {
        // Arrange
        var product = new Product { Stock = 10, IsActive = true };
        var spec = new ProductHasSufficientStockSpecification(5);
        
        // Act
        var result = spec.IsSatisfiedBy(product);
        
        // Assert
        Assert.True(result);
    }
    
    // Más tests...
}
```

---

### TAREA 3: Unit Tests - Validators (Día 2 - 4 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Tests unitarios para todos los validadores FluentValidation

#### Acceptance Criteria:
- [ ] Tests para `CreateSaleCommandValidator`:
  - [ ] Test: SaleDetails vacío falla validación
  - [ ] Test: Quantity <= 0 falla validación
  - [ ] Test: UnitPrice <= 0 falla validación
  - [ ] Test: Discount > subtotal falla validación
  - [ ] Test: AmountPaid < total falla validación
  - [ ] Test: Factura sin cliente falla validación
  - [ ] Test: Productos no existen falla validación
  - [ ] Test: Stock insuficiente falla validación
  - [ ] Test: Productos inactivos falla validación
  - [ ] Test: Cliente no existe falla validación
  - [ ] Test: Happy path pasa validación

- [ ] Tests para `CreateProductCommandValidator`:
  - [ ] Test: Código vacío falla validación
  - [ ] Test: SalePrice <= PurchasePrice falla validación
  - [ ] Test: Stock < 0 falla validación
  - [ ] Test: Categoría no existe falla validación
  - [ ] Test: Happy path pasa validación

- [ ] Tests para `CreateCustomerCommandValidator`:
  - [ ] Test: DNI inválido (no 8 dígitos) falla validación
  - [ ] Test: RUC inválido (no 11 dígitos) falla validación
  - [ ] Test: Teléfono peruano inválido falla validación
  - [ ] Test: Documento duplicado falla validación
  - [ ] Test: Happy path pasa validación

#### Coverage Target: >90% de validadores

#### Reference Files:
- `src/Minimarket.Application/Features/Sales/Commands/CreateSaleCommandValidator.cs`
- `src/Minimarket.Application/Features/Products/Commands/CreateProductCommandValidator.cs`
- `src/Minimarket.Application/Features/Customers/Commands/CreateCustomerCommandValidator.cs`

#### Implementation Notes:
- Usar Moq para mockear `IUnitOfWork`
- Tests deben ser independientes (no compartir estado)
- Usar teorías (Theory) donde sea apropiado para múltiples casos

---

### TAREA 4: Unit Tests - Handlers (Día 2-3 - 6 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Tests unitarios para handlers críticos

#### Acceptance Criteria:
- [ ] Tests para `CreateSaleCommandHandler`:
  - [ ] Test: Crea venta exitosamente (happy path)
  - [ ] Test: Calcula subtotal correctamente
  - [ ] Test: Calcula IGV correctamente (18%)
  - [ ] Test: Calcula total correctamente
  - [ ] Test: Calcula vuelto correctamente
  - [ ] Test: Redondea montos a 2 decimales
  - [ ] Test: Producto no existe lanza NotFoundException
  - [ ] Test: Stock insuficiente lanza InsufficientStockException
  - [ ] Test: Producto inactivo lanza BusinessRuleViolationException
  - [ ] Test: Descuento > subtotal lanza BusinessRuleViolationException
  - [ ] Test: AmountPaid < total lanza BusinessRuleViolationException
  - [ ] Test: Actualiza stock correctamente
  - [ ] Test: Genera número de comprobante único
  - [ ] Test: Rollback en caso de error

- [ ] Tests para `CancelSaleCommandHandler`:
  - [ ] Test: Anula venta exitosamente
  - [ ] Test: Restaura stock correctamente
  - [ ] Test: Venta no existe retorna Failure
  - [ ] Test: Venta ya anulada retorna Failure

#### Coverage Target: >85% de handlers críticos

#### Reference Files:
- `src/Minimarket.Application/Features/Sales/Commands/CreateSaleCommandHandler.cs`
- `src/Minimarket.Application/Features/Sales/Commands/CancelSaleCommandHandler.cs`

#### Implementation Notes:
- Mockear `IUnitOfWork` completamente
- Verificar que se llaman métodos correctos (UpdateAsync, AddAsync, etc.)
- Verificar transacciones (BeginTransaction, Commit, Rollback)

---

### TAREA 5: Integration Tests - Products API (Día 3-4 - 6 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Integration tests para todos los endpoints de Products

#### Acceptance Criteria:
- [ ] Tests para `GET /api/products`:
  - [ ] Test: Retorna lista de productos
  - [ ] Test: Filtro por nombre funciona
  - [ ] Test: Filtro por categoría funciona
  - [ ] Test: Paginación funciona
  - [ ] Test: Requiere autenticación

- [ ] Tests para `GET /api/products/{id}`:
  - [ ] Test: Retorna producto por ID
  - [ ] Test: Producto no existe retorna 404
  - [ ] Test: Requiere autenticación

- [ ] Tests para `POST /api/products`:
  - [ ] Test: Crea producto exitosamente
  - [ ] Test: Validaciones fallan con 400
  - [ ] Test: Código duplicado retorna error
  - [ ] Test: Categoría no existe retorna error
  - [ ] Test: SalePrice <= PurchasePrice retorna error
  - [ ] Test: Requiere autenticación

- [ ] Tests para `PUT /api/products/{id}`:
  - [ ] Test: Actualiza producto exitosamente
  - [ ] Test: Producto no existe retorna 404
  - [ ] Test: Validaciones fallan con 400

- [ ] Tests para `DELETE /api/products/{id}`:
  - [ ] Test: Elimina producto exitosamente
  - [ ] Test: Producto con ventas hace soft delete
  - [ ] Test: Producto no existe retorna 404

#### Coverage Target: 100% de endpoints de Products

#### Reference Files:
- `src/Minimarket.API/Controllers/ProductsController.cs`
- `src/Minimarket.Application/Features/Products/Commands/`

#### Implementation Notes:
- Usar `WebApplicationFactory` para crear test server
- Usar base de datos en memoria para tests
- Seed datos de prueba antes de cada test
- Limpiar datos después de cada test

---

### TAREA 6: Integration Tests - Sales API (Día 4-5 - 6 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Integration tests para todos los endpoints de Sales

#### Acceptance Criteria:
- [ ] Tests para `POST /api/sales`:
  - [ ] Test: Crea venta exitosamente
  - [ ] Test: Actualiza stock correctamente
  - [ ] Test: Calcula totales correctamente
  - [ ] Test: Stock insuficiente retorna error
  - [ ] Test: Producto inactivo retorna error
  - [ ] Test: Factura sin cliente retorna error
  - [ ] Test: AmountPaid < total retorna error
  - [ ] Test: Descuento > subtotal retorna error
  - [ ] Test: Genera número de comprobante único
  - [ ] Test: Rollback en caso de error

- [ ] Tests para `GET /api/sales`:
  - [ ] Test: Retorna lista de ventas
  - [ ] Test: Filtros funcionan correctamente
  - [ ] Test: Paginación funciona

- [ ] Tests para `GET /api/sales/{id}`:
  - [ ] Test: Retorna venta por ID
  - [ ] Test: Venta no existe retorna 404

- [ ] Tests para `POST /api/sales/{id}/cancel`:
  - [ ] Test: Anula venta exitosamente
  - [ ] Test: Restaura stock correctamente
  - [ ] Test: Venta ya anulada retorna error
  - [ ] Test: Venta no existe retorna 404

#### Coverage Target: 100% de endpoints de Sales

#### Reference Files:
- `src/Minimarket.API/Controllers/SalesController.cs`
- `src/Minimarket.Application/Features/Sales/Commands/`

---

### TAREA 7: Integration Tests - Customers API (Día 5 - 4 horas)

**PRIORITY**: 🟠 ALTA  
**DELIVERABLE**: Integration tests para endpoints de Customers

#### Acceptance Criteria:
- [ ] Tests para `GET /api/customers`
- [ ] Tests para `GET /api/customers/{id}`
- [ ] Tests para `POST /api/customers`:
  - [ ] Test: DNI inválido retorna error
  - [ ] Test: RUC inválido retorna error
  - [ ] Test: Documento duplicado retorna error
  - [ ] Test: Teléfono inválido retorna error
- [ ] Tests para `PUT /api/customers/{id}`
- [ ] Tests para `DELETE /api/customers/{id}`

#### Coverage Target: 100% de endpoints de Customers

---

### TAREA 8: Code Coverage Report (Día 5 - 2 horas)

**PRIORITY**: 🔴 CRÍTICA  
**DELIVERABLE**: Reporte de code coverage y análisis

#### Acceptance Criteria:
- [ ] Configurar herramienta de coverage (Coverlet, ReportGenerator)
- [ ] Generar reporte de coverage
- [ ] Verificar que Application layer tiene >80% coverage
- [ ] Identificar áreas sin coverage
- [ ] Documentar gaps de coverage
- [ ] Crear plan para cubrir gaps

#### Tools Recomendados:
- **Coverlet**: Para generar coverage data
- **ReportGenerator**: Para generar reportes HTML
- **dotnet test --collect:"XPlat Code Coverage"**: Para ejecutar tests con coverage

#### Command Example:
```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

---

## ESTRUCTURA DE CARPETAS PARA TESTS

```
tests/
├── Minimarket.UnitTests/
│   ├── Domain/
│   │   └── Specifications/
│   │       ├── ProductHasSufficientStockSpecificationTests.cs
│   │       ├── ProductIsActiveSpecificationTests.cs
│   │       └── SaleCanBeCancelledSpecificationTests.cs
│   ├── Application/
│   │   ├── Features/
│   │   │   ├── Sales/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateSaleCommandHandlerTests.cs
│   │   │   │   │   ├── CreateSaleCommandValidatorTests.cs
│   │   │   │   │   └── CancelSaleCommandHandlerTests.cs
│   │   │   ├── Products/
│   │   │   │   └── Commands/
│   │   │   │       ├── CreateProductCommandValidatorTests.cs
│   │   │   │       └── UpdateProductCommandValidatorTests.cs
│   │   │   └── Customers/
│   │   │       └── Commands/
│   │   │           └── CreateCustomerCommandValidatorTests.cs
│   │   └── Common/
│   │       └── Behaviors/
│   │           └── ValidationBehaviorTests.cs
│   └── Helpers/
│       └── TestDataBuilder.cs
│
├── Minimarket.IntegrationTests/
│   ├── Controllers/
│   │   ├── ProductsControllerTests.cs
│   │   ├── SalesControllerTests.cs
│   │   ├── CustomersControllerTests.cs
│   │   └── CategoriesControllerTests.cs
│   ├── BaseIntegrationTest.cs
│   ├── TestFixture.cs
│   └── Helpers/
│       ├── DatabaseSeeder.cs
│       └── TestDataHelper.cs
│
└── Minimarket.FunctionalTests/
    └── (Para tests funcionales end-to-end si es necesario)
```

---

## ESTÁNDARES DE TESTING

### Naming Conventions
- **Test Classes**: `[ClassUnderTest]Tests.cs`
- **Test Methods**: `[MethodName]_[Scenario]_[ExpectedResult]` o `[Scenario]_[ExpectedResult]`
- **Ejemplo**: `CreateSaleCommandHandler_CreateSale_WithValidRequest_ReturnsSuccess`

### Test Structure (AAA Pattern)
```csharp
[Fact]
public void CreateSale_WithValidRequest_ReturnsSuccess()
{
    // Arrange
    var command = new CreateSaleCommand { /* ... */ };
    var handler = new CreateSaleCommandHandler(/* ... */);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
}
```

### Test Data Builders
- Crear builders para facilitar creación de datos de prueba
- Ejemplo: `ProductBuilder`, `SaleBuilder`, `CustomerBuilder`

### Mocking
- Usar **Moq** para mockear dependencias
- Mock solo lo necesario (no over-mock)
- Verificar interacciones cuando sea relevante

---

## MÉTRICAS Y OBJETIVOS

### Coverage Targets
- **Application Layer**: >80% (CRÍTICO)
- **Domain Layer**: >90% (especificaciones)
- **Validators**: >90%
- **Handlers**: >85%

### Test Count Targets
- **Unit Tests**: Mínimo 50 tests
- **Integration Tests**: Mínimo 30 tests
- **Total**: Mínimo 80 tests

### Quality Metrics
- **Tests Passing**: 100%
- **Tests Execution Time**: <30 segundos para suite completa
- **Test Independence**: Cada test debe poder ejecutarse solo

---

## DEPENDENCIAS Y BLOQUEOS

### Dependencias
- ✅ Proyectos de tests ya existen
- ✅ Código a testear está implementado
- ⚠️ Necesita verificar frameworks instalados

### Bloqueos Potenciales
- Si falta configuración de WebApplicationFactory
- Si falta configuración de test database
- Si hay problemas con dependencias de packages

### Acción si Bloqueado
- Reportar inmediatamente a Tech Lead
- Documentar el bloqueo específico
- Proponer solución alternativa

---

## REPORTE DIARIO REQUERIDO

Al final de cada día, reportar:

```
## DAILY PROGRESS - QA Backend - [Fecha]

### Tests Escritos Hoy:
- Unit Tests: X tests
- Integration Tests: Y tests
- Total: X + Y tests

### Coverage Actual:
- Application Layer: X%
- Domain Layer: Y%
- Total: Z%

### Tests Passing:
- ✅ Todos pasando / ⚠️ X tests fallando

### Blockers:
- [Lista de blockers si los hay]

### Plan Mañana:
- [Tareas específicas para mañana]
```

---

## ACCEPTANCE CRITERIA FINAL

El trabajo está **COMPLETO** cuando:

- [ ] ✅ Todos los tests unitarios escritos y pasando
- [ ] ✅ Todos los integration tests escritos y pasando
- [ ] ✅ Code coverage >80% en Application layer
- [ ] ✅ Reporte de coverage generado y documentado
- [ ] ✅ Todos los tests ejecutan en <30 segundos
- [ ] ✅ Tests son independientes y pueden ejecutarse solos
- [ ] ✅ Documentación de tests actualizada
- [ ] ✅ PR creado con todos los tests
- [ ] ✅ Code review aprobado por Tech Lead

---

## RECURSOS Y REFERENCIAS

### Documentación
- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [FluentAssertions](https://fluentassertions.com/)
- [ASP.NET Core Integration Tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)

### Archivos de Referencia
- `src/Minimarket.Application/Features/` - Código a testear
- `src/Minimarket.API/Controllers/` - Endpoints a testear
- `CODE_REVIEW_BusinessLogicValidator.md` - Contexto de validaciones

---

## PRIORIZACIÓN DE TAREAS

**Orden de Ejecución Recomendado**:
1. **Día 1**: Tarea 1 (Setup) → Tarea 2 (Specifications)
2. **Día 2**: Tarea 3 (Validators) → Tarea 4 (Handlers - inicio)
3. **Día 3**: Tarea 4 (Handlers - completar) → Tarea 5 (Products API - inicio)
4. **Día 4**: Tarea 5 (Products API - completar) → Tarea 6 (Sales API - inicio)
5. **Día 5**: Tarea 6 (Sales API - completar) → Tarea 7 (Customers API) → Tarea 8 (Coverage Report)

---

## NOTAS FINALES

**@QA-Backend**: 

Esta es una tarea **CRÍTICA** que bloquea el avance del proyecto. El código actualmente tiene 0% coverage, lo cual es inaceptable para un proyecto de producción.

**ENFÓCATE EN**:
- ✅ Calidad sobre cantidad (pero necesitamos cantidad mínima)
- ✅ Tests significativos que capturen bugs reales
- ✅ Coverage en lógica de negocio crítica primero
- ✅ Mantener tests simples y legibles

**NO TE PREOCUPES POR**:
- ❌ Coverage 100% (objetivo es >80%)
- ❌ Tests para código trivial (getters/setters)
- ❌ Tests complejos que son difíciles de mantener

**SI TIENES DUDAS**:
- Consulta con Tech Lead inmediatamente
- No pierdas tiempo en dudas técnicas
- Prioriza avanzar sobre perfeccionar

**ESTA TAREA ES TU PRIORIDAD #1 ESTA SEMANA. TODO LO DEMÁS ES SECUNDARIO.**

---

**ASIGNADO POR**: Tech Lead  
**FECHA**: [Fecha Actual]  
**DEADLINE**: [Fecha + 5 días hábiles]  
**STATUS**: 🟡 EN PROGRESO

